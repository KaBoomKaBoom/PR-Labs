using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

public enum NodeState
{
    Follower,
    Candidate,
    Leader
}

public class RaftNode
{
    private NodeState _state = NodeState.Follower;
    private readonly int _port;
    private readonly string _id;
    private readonly List<string> _peers;
    private readonly UdpClient _udpClient;
    private readonly Random _random = new();

    // Diagnostic logging
    private void DiagnosticLog(string message)
    {
        Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}] {_id}: {message}");
    }

    // Election and heartbeat parameters
    private int _currentTerm = 0;
    private string _votedFor = null;
    private DateTime _lastHeartbeatTime = DateTime.UtcNow;
    private int _electionTimeout;
    private const int BaseElectionTimeoutMs = 3000;
    private const int HeartbeatIntervalMs = 1500;

    private CancellationTokenSource _cancellationTokenSource;
    private ConcurrentDictionary<string, bool> _votesReceived = new();
    private readonly object _lock = new();

    public RaftNode(int port, string id, List<string> peers)
    {
        _port = port;
        _id = id;
        _peers = peers;
        _udpClient = new UdpClient(port, AddressFamily.InterNetwork);

        // Randomize election timeout
        _electionTimeout = _random.Next(BaseElectionTimeoutMs, BaseElectionTimeoutMs * 2);

        DiagnosticLog($"Initialized with port {port}, actual local endpoint: {_udpClient.Client.LocalEndPoint}, peers: {string.Join(", ", peers)}");
    }

    public void Start()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        Task.Run(() => ListenForMessages(_cancellationTokenSource.Token));
        Task.Run(() => ManageNodeState(_cancellationTokenSource.Token));
    }

    private async Task ListenForMessages(CancellationToken token)
    {
        // Bind to all network interfaces
        var listener = new UdpClient(new IPEndPoint(IPAddress.Any, _port));

        while (!token.IsCancellationRequested)
        {
            try
            {
                UdpReceiveResult result = await listener.ReceiveAsync();
                var message = Encoding.UTF8.GetString(result.Buffer);

                DiagnosticLog($"Received RAW message from {result.RemoteEndPoint}: {message}");

                // Additional network diagnostic
                DiagnosticLog($"Message details - Local Port: {_port}, Sender Port: {result.RemoteEndPoint.Port}");

                ProcessMessage(message, result.RemoteEndPoint);
            }
            catch (Exception ex)
            {
                DiagnosticLog($"Comprehensive receive error: {ex}");
                await Task.Delay(100);
            }
        }
    }

    private void HandleVoteRequest(int term, string candidateId, IPEndPoint remoteEndPoint)
    {
        bool voteGranted = (_votedFor == null || _votedFor == candidateId) && term >= _currentTerm;

        DiagnosticLog($"Vote request details: term={term}, candidateId={candidateId}, currentVotedFor={_votedFor}, voteGranted={voteGranted}");

        if (voteGranted)
        {
            _currentTerm = term;
            _votedFor = candidateId;
            _lastHeartbeatTime = DateTime.UtcNow;
            SendMessage(remoteEndPoint, $"{_id}|Vote|{term}");
            DiagnosticLog($"Voted for {candidateId} in term {term}");
        }
        else
        {
            DiagnosticLog($"Rejected vote request from {candidateId} in term {term}");
            SendMessage(remoteEndPoint, $"{_id}|VoteRejected|{term}");
        }
    }

    private void HandleVoteResponse(int term, string voterId)
    {
        if (_state != NodeState.Candidate)
        {
            DiagnosticLog($"Received vote from {voterId}, but not in candidate state");
            return;
        }

        _votesReceived[voterId] = true;
        int votesCount = _votesReceived.Count;

        DiagnosticLog($"Vote received from {voterId}. Total votes: {votesCount}/{_peers.Count + 1}");

        if (votesCount > (_peers.Count / 2))
        {
            _state = NodeState.Leader;
            _votedFor = _id;
            DiagnosticLog($"Elected as leader in term {_currentTerm}");
            StartHeartbeats();
        }
    }

    private void HandleHeartbeat(int term, string leaderId)
    {
        _lastHeartbeatTime = DateTime.UtcNow;
        _state = NodeState.Follower;
        _votedFor = null;
        DiagnosticLog($"Received heartbeat from {leaderId} in term {term}");
    }

    private void ProcessMessage(string message, IPEndPoint remoteEndPoint)
    {
        var parts = message.Split('|');
        if (parts.Length < 3)
        {
            DiagnosticLog($"Invalid message format: {message}");
            return;
        }

        var senderId = parts[0];
        var messageType = parts[1];
        int term;

        if (!int.TryParse(parts[2], out term))
        {
            DiagnosticLog($"Failed to parse term from message: {message}");
            return;
        }

        lock (_lock)
        {
            // More verbose logging around term and state changes
            DiagnosticLog($"Processing {messageType} from {senderId}, remote term: {term}, current term: {_currentTerm}, current state: {_state}");

            // Update term if necessary with more explicit logic
            if (term > _currentTerm)
            {
                DiagnosticLog($"Term update: {_currentTerm} -> {term}");
                _currentTerm = term;
                _state = NodeState.Follower;
                _votedFor = null;
            }

            switch (messageType)
            {
                case "VoteRequest":
                    HandleVoteRequest(term, senderId, remoteEndPoint);
                    break;

                case "Vote":
                    HandleVoteResponse(term, senderId);
                    break;

                case "Heartbeat":
                    HandleHeartbeat(term, senderId);
                    break;

                case "VoteRejected":
                    DiagnosticLog($"Vote rejected in term {term}");
                    break;
            }
        }
    }

    private async Task ManageNodeState(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                var timeSinceLastHeartbeat = (DateTime.UtcNow - _lastHeartbeatTime).TotalMilliseconds;

                if (timeSinceLastHeartbeat > _electionTimeout)
                {
                    StartElection();

                    // Randomize election timeout after each election attempt
                    _electionTimeout = _random.Next(BaseElectionTimeoutMs, BaseElectionTimeoutMs * 2);
                }

                // Shorter delay to check more frequently
                await Task.Delay(500, token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private void StartElection()
    {
        _state = NodeState.Candidate;
        _currentTerm++;
        _votesReceived.Clear();
        _votedFor = _id;
        _votesReceived[_id] = true;

        DiagnosticLog($"Starting election in term {_currentTerm}");

        foreach (var peer in _peers)
        {
            var parts = peer.Split(':');
            var ip = IPAddress.Parse(parts[0]);
            var port = int.Parse(parts[1]);

            try
            {
                // Create endpoint for each peer
                var endPoint = new IPEndPoint(ip, port);

                // Diagnostic: show exact endpoint being used
                DiagnosticLog($"Attempting to send vote request to endpoint: {endPoint}");

                SendMessage(endPoint, $"{_id}|VoteRequest|{_currentTerm}");
            }
            catch (Exception ex)
            {
                DiagnosticLog($"Comprehensive election error for peer {peer}: {ex}");
            }
        }
    }

    private void StartHeartbeats()
    {
        Task.Run(async () =>
        {
            while (_state == NodeState.Leader)
            {
                foreach (var peer in _peers)
                {
                    var parts = peer.Split(':');
                    var ip = parts[0];
                    var port = int.Parse(parts[1]);

                    try
                    {
                        var endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
                        SendMessage(endPoint, $"{_id}|Heartbeat|{_currentTerm}");
                    }
                    catch (Exception ex)
                    {
                        DiagnosticLog($"Error sending heartbeat to {ip}:{port}: {ex.Message}");
                    }
                }

                await Task.Delay(HeartbeatIntervalMs);
            }
        });
    }

    private void SendMessage(IPEndPoint endPoint, string message)
    {
        try
        {
            var bytes = Encoding.UTF8.GetBytes(message);

            // Try sending to specific endpoint first
            try
            {
                _udpClient.Send(bytes, bytes.Length, endPoint);
                DiagnosticLog($"Sent message to specific endpoint {endPoint}: {message}");
            }
            catch
            {
                // If sending to specific endpoint fails, broadcast
                var broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, endPoint.Port);
                _udpClient.Send(bytes, bytes.Length, broadcastEndpoint);
                DiagnosticLog($"Sent broadcast message to {broadcastEndpoint}: {message}");
            }
        }
        catch (Exception ex)
        {
            DiagnosticLog($"Comprehensive send error to {endPoint}: {ex}");
        }
    }

    public void Stop()
    {
        _cancellationTokenSource.Cancel();
        _udpClient.Close();
    }
}
