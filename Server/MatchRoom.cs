// Trong Server.cs

using System.Net.Sockets;

class MatchRoom
{
    public List<TcpClient> Clients { get; set; }
    public int StartingPlayer { get; set; } // 1 hoặc 2

    public MatchRoom()
    {
        Clients = new List<TcpClient>();
        StartingPlayer = 1; // Mặc định người chơi đầu tiên là 1
    }

    // Phương thức để luân phiên người bắt đầu
    public void ToggleStartingPlayer()
    {
        StartingPlayer = (StartingPlayer == 1) ? 2 : 1;
    }
}