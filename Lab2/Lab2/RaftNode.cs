using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class RaftNode
{
    private enum State { Follower, Candidate, Leader }

    private readonly string nodeId;
    private readonly int port;
    private readonly string[] peers;
    private State state;
    private int currentTerm;
    private string votedFor;
    private UdpClient udpClient;
    private CancellationTokenSource cts;
    private Random random = new Random();
    private readonly int electionTimeoutMin = 150;
    private readonly int electionTimeoutMax = 300;
    private DateTime lastHeartbeat;
    private readonly object lockObject = new object();
    private Timer heartbeatTimer;
    private const int HeartbeatInterval = 100; // ms
    private int votesReceived;

    public RaftNode(string nodeId, int port, string[] peers)
    {
        this.nodeId = nodeId;
        this.port = port;
        this.peers = peers;
        state = State.Follower;
        currentTerm = 0;
        votedFor = null;
        lastHeartbeat = DateTime.UtcNow;
    }

    public async Task StartAsync()
    {
        udpClient = new UdpClient(port);
        cts = new CancellationTokenSource();

        Console.WriteLine($"{nodeId} started on port {port}");

        var listenerTask = ListenAsync(cts.Token);
        var electionTask = StartElectionTimeoutAsync(cts.Token);

        await Task.WhenAll(listenerTask, electionTask);
    }

    private async Task ListenAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                var result = await udpClient.ReceiveAsync(token);
                var message = Encoding.UTF8.GetString(result.Buffer);
                HandleMessage(message, result.RemoteEndPoint);
            }
            catch (Exception ex) when (ex is OperationCanceledException || ex is ObjectDisposedException)
            {
                // Graceful shutdown
            }
        }
    }

    private void HandleMessage(string message, IPEndPoint sender)
    {
        var parts = message.Split('|');
        var type = parts[0];
        var term = int.Parse(parts[1]);

        if (term > currentTerm)
        {
            currentTerm = term;
            state = State.Follower;
            votedFor = null;
        }

        switch (type)
        {
            case "RequestVote":
                HandleRequestVote(parts, sender);
                break;
            case "VoteGranted":
                if (state == State.Candidate)
                {
                    Console.WriteLine($"{nodeId} received vote from {parts[2]}");
                    if (HaveReceivedMajorityVotes())
                    {
                        BecomeLeader();
                    }
                }
                break;
            case "Heartbeat":
                HandleHeartbeat(parts);
                break;
        }
    }

    private bool HaveReceivedMajorityVotes()
    {
        int voteCount = 1; // Start with 1 for the current node
        foreach (var peer in peers)
        {
            var parts = peer.Split(':');
            var hostname = parts[0]; // Use the hostname from PEERS
            var port = int.Parse(parts[1]);

            // Simulating receiving a vote response from each peer (in reality, these would be UDP messages)
            if (peer != nodeId) // Exclude own node
            {
                voteCount++;
            }
        }

        int majority = (peers.Length / 2) + 1;
        return voteCount >= majority;
    }

    private void HandleRequestVote(string[] parts, IPEndPoint sender)
    {
        lock (lockObject)
        {
            var candidateTerm = int.Parse(parts[1]);
            var candidateId = parts[2];

            if (candidateTerm > currentTerm)
            {
                currentTerm = candidateTerm;
                state = State.Follower;
                votedFor = null;
            }

            if (candidateTerm >= currentTerm && (votedFor == null || votedFor == candidateId))
            {
                votedFor = candidateId;
                var response = $"VoteGranted|{currentTerm}|{nodeId}";
                udpClient.SendAsync(Encoding.UTF8.GetBytes(response), response.Length, sender);
                Console.WriteLine($"{nodeId} voted for {candidateId}");
            }
        }
    }

    private void StartElection()
    {
        lock (lockObject)
        {
            if (state == State.Leader)
                return;

            state = State.Candidate;
            currentTerm++;
            votedFor = nodeId;
            votesReceived = 1; // Vote for self
            lastHeartbeat = DateTime.UtcNow;

            Console.WriteLine($"{nodeId} started election for term {currentTerm}");
            foreach (var peer in peers)
            {
                SendRequestVote(peer);
            }
        }
    }

    private void BecomeLeader()
    {
        lock (lockObject)
        {
            state = State.Leader;
            Console.WriteLine($"{nodeId} is now the leader of term {currentTerm}");
            
            // Start sending periodic heartbeats
            heartbeatTimer = new Timer(_ => SendHeartbeats(), null, 0, HeartbeatInterval);
        }
    }

    private void SendHeartbeats()
    {
        if (state != State.Leader) return;

        foreach (var peer in peers)
        {
            var parts = peer.Split(':');
            var hostname = parts[0];
            var port = int.Parse(parts[1]);

            var message = $"Heartbeat|{currentTerm}|{nodeId}";
            var messageBytes = Encoding.UTF8.GetBytes(message);
            
            try
            {
                udpClient.SendAsync(messageBytes, messageBytes.Length, hostname, port);
                Console.WriteLine($"{nodeId} sending heartbeat to {hostname}:{port}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending heartbeat to {peer}: {ex.Message}");
            }
        }
    }

    private void HandleHeartbeat(string[] parts)
    {
        lock (lockObject)
        {
            var leaderTerm = int.Parse(parts[1]);
            if (leaderTerm >= currentTerm)
            {
                currentTerm = leaderTerm;
                state = State.Follower;
                votedFor = null;
                votesReceived = 0;
                lastHeartbeat = DateTime.UtcNow;
                Console.WriteLine($"{nodeId} received heartbeat from leader {parts[2]}");
            }
        }
    }

    private async Task StartElectionTimeoutAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            var timeout = random.Next(electionTimeoutMin, electionTimeoutMax);
            try
            {
                await Task.Delay(timeout, token);
                
                lock (lockObject)
                {
                    var timeSinceLastHeartbeat = DateTime.UtcNow - lastHeartbeat;
                    if (state != State.Leader && 
                        timeSinceLastHeartbeat.TotalMilliseconds > electionTimeoutMax && 
                        DateTime.UtcNow - lastHeartbeat > TimeSpan.FromMilliseconds(electionTimeoutMax))
                    {
                        StartElection();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Token cancelled, exit gracefully
            }
        }
    }

    private void SendRequestVote(string peer)
    {
        var parts = peer.Split(':');
        var hostname = parts[0];
        var port = int.Parse(parts[1]);

        var message = $"RequestVote|{currentTerm}|{nodeId}";
        var messageBytes = Encoding.UTF8.GetBytes(message);
        
        try 
        {
            udpClient.SendAsync(messageBytes, messageBytes.Length, hostname, port)
                .ContinueWith(t => 
                {
                    if (t.IsFaulted)
                    {
                        Console.WriteLine($"Failed to send RequestVote to {peer}: {t.Exception}");
                    }
                });
            
            Console.WriteLine($"{nodeId} sending RequestVote to {hostname}:{port}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending RequestVote to {peer}: {ex.Message}");
        }
    }

    public void Stop()
    {
        heartbeatTimer?.Dispose();
        cts.Cancel();
        udpClient.Close();
        Console.WriteLine($"{nodeId} stopped.");
    }
}
