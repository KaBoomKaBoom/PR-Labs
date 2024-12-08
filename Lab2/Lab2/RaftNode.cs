//TODO 1: When a node stops, delete its address from the list of peers of the remaining nodes.


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
    private string[] peers;
    private State state;
    private int currentTerm;
    private string votedFor;
    private UdpClient udpClient;
    private CancellationTokenSource cts;
    private Random random = new Random();
    private readonly int electionTimeoutMin = 2000;
    private readonly int electionTimeoutMax = 4000;
    private DateTime lastHeartbeat;
    private readonly object lockObject = new object();
    private Timer heartbeatTimer;
    private const int HeartbeatInterval = 200; // ms
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
        await Task.Delay(3000);
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

        if (type == "NodeStopped")
        {
            HandleNodeStopped(parts[1]);
            return;
        }
        var term = int.Parse(parts[1]);

        if (term > currentTerm)
        {
            currentTerm = term;
            state = State.Follower;
            votedFor = null;
            Console.WriteLine($"{nodeId} stepping down to Follower for term {currentTerm}");
        }

        switch (type)
        {
            case "RequestVote":
                HandleRequestVote(parts, sender);
                break;
            case "VoteGranted":
                if (state == State.Candidate)
                {
                    votesReceived++;
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
            case "NodeStopped":
                HandleNodeStopped(parts[1]);
                break;

        }
    }
    private void HandleNodeStopped(string stoppedNodeId)
    {
        lock (lockObject)
        {
            Console.WriteLine($"{nodeId} processing NodeStopped for {stoppedNodeId}");
            var updatedPeers = peers.Where(peer => !peer.StartsWith("lab2-"+stoppedNodeId)).ToArray();

            if (updatedPeers.Length != peers.Length)
            {
                Console.WriteLine($"{nodeId} removed {stoppedNodeId} from peer list.");
            }
            else
            {
                Console.WriteLine($"{nodeId} did not find {stoppedNodeId} in peer list.");
            }

            peers = updatedPeers;
        }
    }


    private bool HaveReceivedMajorityVotes()
    {
        int totalNodes = GlobalState.PeerCount + 1; // Include self
        int voteCount = votesReceived;
        int majority = (totalNodes / 2) + 1;
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
            if (state == State.Leader) return; // Prevent multiple leader transitions

            state = State.Leader;
            Console.WriteLine($"{nodeId} is now the leader of term {currentTerm}");

            // Stop election timeout as the node is now the leader
            lastHeartbeat = DateTime.UtcNow;

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
            var leaderId = parts[2];

            if (leaderTerm >= currentTerm)
            {
                currentTerm = leaderTerm;
                state = State.Follower; // Ensure state transitions to Follower
                votedFor = null;
                votesReceived = 0;
                lastHeartbeat = DateTime.UtcNow; // Update the last heartbeat time
                Console.WriteLine($"{nodeId} received heartbeat from leader {leaderId}");
            }
            else
            {
                Console.WriteLine($"{nodeId} ignored outdated heartbeat from {leaderId}");
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
                    if (state == State.Leader) continue;

                    // If no heartbeat has been received within the timeout, start a new election
                    var timeSinceLastHeartbeat = DateTime.UtcNow - lastHeartbeat;
                    if (timeSinceLastHeartbeat.TotalMilliseconds > timeout)
                    {
                        Console.WriteLine($"{nodeId} detected heartbeat timeout. Starting election.");
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
            udpClient.SendAsync(messageBytes, messageBytes.Length, hostname, port);
            Console.WriteLine($"{nodeId} requested vote from {hostname}:{port}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending request vote to {peer}: {ex.Message}");
        }
    }
    private void NotifyPeersOfShutdown()
    {
        foreach (var peer in peers)
        {
            var parts = peer.Split(':');
            var hostname = parts[0];
            var port = int.Parse(parts[1]);

            var message = $"NodeStopped|{nodeId}";
            var messageBytes = Encoding.UTF8.GetBytes(message);

            try
            {
                udpClient.SendAsync(messageBytes, messageBytes.Length, hostname, port);
                Console.WriteLine($"{nodeId} notifying shutdown to {hostname}:{port}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error notifying shutdown to {peer}: {ex.Message}");
            }
        }
    }

    public void Stop()
    {
        NotifyPeersOfShutdown();
        cts.Cancel();
        udpClient.Dispose();
        heartbeatTimer?.Dispose();
        GlobalState.DecrementPeerCount();
        Console.WriteLine($"{nodeId} stopped.");
    }
}
