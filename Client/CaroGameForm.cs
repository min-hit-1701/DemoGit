using System;
using System.Drawing;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Client
{
    public partial class CaroGameForm : Form
    {
        private Button[,] board;
        private const int BoardSize = 10; // Kích thước bàn cờ
        private bool isMyTurn = false; // Kiểm soát lượt đi
        private string mySymbol = ""; // Quân cờ, sẽ được nhận từ server ("X" hoặc "O")
        private TcpClient client;
        private NetworkStream stream;

        public CaroGameForm()
        {
            InitializeComponent();
            this.Load += CaroGameForm_Load;
        }

        private void CaroGameForm_Load(object sender, EventArgs e)
        {
            buttonConnect = new Button
            {
                Location = new Point(10, 10),
                Width = 100,
                Text = "Kết nối"
            };
            buttonConnect.Click += ButtonConnect_Click;

            this.Controls.Add(buttonConnect);
            InitializeBoard(BoardSize);
        }

        private void ButtonConnect_Click(object sender, EventArgs e)
        {
            try
            {
                ConnectToServer("127.0.0.1", 12345);
                MessageBox.Show("Đã kết nối thành công tới server.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể kết nối tới server: {ex.Message}");
            }

            // Sau khi kết nối, gửi yêu cầu tham gia trận đấu
            string matchCode = textBoxMatchCode.Text.Trim();
            if (string.IsNullOrEmpty(matchCode))
            {
                MessageBox.Show("Vui lòng nhập mã trận đấu.");
                return;
            }

            try
            {
                // Gửi yêu cầu tham gia trận đấu tới server
                string joinMessage = $"JOIN:{matchCode}";
                byte[] data = Encoding.UTF8.GetBytes(joinMessage);
                stream.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tham gia trận đấu: {ex.Message}");
            }
        }

        private void InitializeBoard(int boardSize)
        {
            board = new Button[boardSize, boardSize];
            panelBoard.Controls.Clear();
            int cellSize = Math.Min(panelBoard.ClientSize.Width / boardSize, panelBoard.ClientSize.Height / boardSize);
            panelBoard.Width = boardSize * cellSize;
            panelBoard.Height = boardSize * cellSize;

            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    Button btn = new Button
                    {
                        Width = cellSize,
                        Height = cellSize,
                        Location = new Point(j * cellSize, i * cellSize),
                        Font = new Font("Arial", 16, FontStyle.Bold),
                        Tag = new Point(i, j)
                    };
                    btn.Click += Btn_Click;
                    panelBoard.Controls.Add(btn);
                    board[i, j] = btn;
                }
            }
        }

        private void Btn_Click(object sender, EventArgs e)
        {
            if (!isMyTurn)
            {
                MessageBox.Show("Chưa đến lượt bạn!");
                return;
            }

            Button btn = (Button)sender;
            if (btn != null && string.IsNullOrEmpty(btn.Text))
            {
                btn.Text = mySymbol;
                var point = (Point)btn.Tag;
                SendMoveToServer(point.X, point.Y);

                // Kiểm tra thắng thua sau khi đánh
                if (CheckWinCondition(point.X, point.Y, mySymbol))
                {
                    MessageBox.Show($"Người chơi {mySymbol} đã thắng!");
                    SendResetCommand(); // Gửi lệnh reset đến server để xóa bàn cờ của cả hai client
                }

                isMyTurn = false; // Sau khi đánh, chuyển lượt
            }
        }

        private void SendMoveToServer(int x, int y)
        {
            string moveMessage = $"MOVE:{x},{y},{mySymbol}";
            byte[] data = Encoding.UTF8.GetBytes(moveMessage);
            stream.Write(data, 0, data.Length);
        }

        private void SendResetCommand()
        {
            string resetMessage = "RESET:";
            byte[] data = Encoding.UTF8.GetBytes(resetMessage);
            stream.Write(data, 0, data.Length);
        }

        private void ConnectToServer(string ip, int port)
        {
            client = new TcpClient();
            client.Connect(ip, port);
            stream = client.GetStream();

            // Bắt đầu lắng nghe dữ liệu từ server
            var listenThread = new Thread(ListenForData);
            listenThread.Start();
        }

        private void ListenForData()
        {
            byte[] buffer = new byte[1024];
            while (true)
            {
                try
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        string[] parts = data.Split(':');

                        if (parts[0] == "MOVE")
                        {
                            string[] moveParts = parts[1].Split(',');
                            int x = int.Parse(moveParts[0]);
                            int y = int.Parse(moveParts[1]);
                            string symbol = moveParts[2];

                            if (this.IsHandleCreated)
                            {
                                this.BeginInvoke(new Action(() =>
                                {
                                    board[x, y].Text = symbol;
                                }));
                            }
                        }
                        else if (parts[0] == "TURN")
                        {
                            this.BeginInvoke(new Action(() =>
                            {
                                isMyTurn = true;
                                mySymbol = parts[1] == "1" ? "X" : "O";
                            }));
                        }
                        else if (parts[0] == "RESET")
                        {
                            this.BeginInvoke(new Action(() =>
                            {
                                ResetBoard();
                            }));
                        }
                        else if (parts[0] == "TIMEOUT")
                        {
                            // Xử lý khi đối thủ hết thời gian
                            this.BeginInvoke(new Action(() =>
                            {
                                MessageBox.Show("Đối thủ đã hết thời gian! Bạn thắng.");
                                SendResetCommand(); // Hoặc xử lý khác tùy theo yêu cầu
                            }));
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                    break;
                }
            }
        }

        private bool CheckWinCondition(int row, int col, string symbol)
        {
            return CheckLine(row, col, 1, 0, symbol) || // Kiểm tra hàng ngang
                   CheckLine(row, col, 0, 1, symbol) || // Kiểm tra hàng dọc
                   CheckLine(row, col, 1, 1, symbol) || // Kiểm tra chéo chính
                   CheckLine(row, col, 1, -1, symbol);  // Kiểm tra chéo phụ
        }

        private bool CheckLine(int row, int col, int dRow, int dCol, string symbol)
        {
            int count = 1; // Đếm quân của người chơi tại vị trí hiện tại
            count += CountConsecutive(row, col, dRow, dCol, symbol);
            count += CountConsecutive(row, col, -dRow, -dCol, symbol);
            return count >= 5; // Điều kiện thắng là 5 quân liên tiếp
        }

        private int CountConsecutive(int row, int col, int dRow, int dCol, string symbol)
        {
            int count = 0;
            for (int i = 1; i < 5; i++) // Kiểm tra đến tối đa 4 ô mỗi hướng
            {
                int r = row + i * dRow;
                int c = col + i * dCol;
                if (r >= 0 && r < BoardSize && c >= 0 && c < BoardSize && board[r, c].Text == symbol)
                {
                    count++;
                }
                else
                {
                    break; // Ngừng nếu không khớp
                }
            }
            return count;
        }

        private void ResetBoard()
        {
            foreach (Button btn in panelBoard.Controls)
            {
                btn.Text = ""; // Xóa toàn bộ quân cờ trên bàn cờ
            }
            isMyTurn = false; // Đặt lại trạng thái lượt chơi
            mySymbol = ""; // Reset quân cờ
        }
    }
}