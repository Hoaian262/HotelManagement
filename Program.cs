using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace HotelAppFinal
{
    public enum Gender { Nam, Nu, Khac }
    public struct Customer
    {
        public int Id;
        public string Ten;
        public string CCCD;
        public string SDT;
        public Gender GioiTinh;
    }
    public class Room
    {
        public int SoPhong { get; set; }
        public int Tang { get; set; }
        public decimal Gia { get; set; }
        public int SucChua { get; set; }
        public Room(int soPhong, int tang, decimal gia, int sucChua)
        {
            SoPhong = soPhong; Tang = tang; Gia = gia; SucChua = sucChua;
        }
        public override string ToString()
        {
            return $"P.{SoPhong} | Tầng {Tang} | {Gia:N0} VNĐ | {SucChua} người";
        }
    }
    public class Booking
    {
        public int Id { get; set; }
        public int RoomNo { get; set; }
        public int CustomerId { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public decimal Total { get; set; }
        public Booking(int id, int roomNo, int custId, DateTime checkIn, DateTime checkOut, decimal total)
        {
            Id = id; RoomNo = roomNo; CustomerId = custId; CheckIn = checkIn; CheckOut = checkOut; Total = total;
        }
    }
    class Program
    {
        static int[,] roomMap = new int[5, 3];
        static List<Room> rooms = new();
        static List<Customer> customers = new();
        static List<Booking> bookings = new();
        const string ROOMS_FILE = "rooms.txt";
        const string CUST_FILE = "customers.txt";
        const string BOOK_FILE = "bookings.txt";
        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8; // Hỗ trợ nhập tiếng Việt

            InitRooms();
            LoadData();
            BuildRoomMap();

            bool running = true;
            while (running)
            {
                ShowMenu();
                // FIX: Dùng hàm nhập số để chọn menu, tránh lỗi khi nhập chữ
                int choice = EnterInt("Chọn chức năng (1-8): ");
                switch (choice)
                {
                    case 1: ShowMapMatrix(); break;
                    case 2: ShowRooms(); break;
                    case 3: BookRoom(); break;
                    case 4:
                        SortBookingsByPrice();
                        Report();
                        break;
                    case 5: ShowBookingsByDateAsc(); break;
                    case 6: ManageData(); break; // Đã viết lại bên dưới
                    case 7: SaveData(); break;
                    case 8: running = false; break;
                    default: LogError("Vui lòng chọn từ 1 đến 8!"); break;
                }
                if (running)
                {
                    Console.WriteLine("\nẤn phím bất kỳ để tiếp tục...");
                    Console.ReadKey();
                }
            }
        }
        static void ShowMenu()
        {
            Console.Clear(); // Xóa màn hình cho sạch
            Console.WriteLine("=== QUẢN LÝ KHÁCH SẠN ===");
            Console.WriteLine("1. Xem sơ đồ khách sạn");
            Console.WriteLine("2. Thông tin phòng");
            Console.WriteLine("3. Đặt phòng");
            Console.WriteLine("4. Báo cáo doanh thu");
            Console.WriteLine("5. Danh sách đặt phòng");
            Console.WriteLine("6. Quản lý dữ liệu");
            Console.WriteLine("7. Lưu dữ liệu");
            Console.WriteLine("8. Thoát");
        }
        static int EnterInt(string msg)
        {
            int value;
            while (true)
            {
                Console.Write(msg);
                string input = Console.ReadLine();
                if (int.TryParse(input, out value)) return value;
                Console.WriteLine(">> Lỗi: Vui lòng nhập một số nguyên!");
            }
        }
        // Hàm nhập chuỗi không được để trống
        static string EnterString(string msg)
        {
            while (true)
            {
                Console.Write(msg);
                string input = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(input)) return input.Trim();
                Console.WriteLine(">> Lỗi: Không được để trống!");
            }
        }
        // Hàm nhập Enum Giới tính
        static Gender EnterGender()
        {
            Console.WriteLine("Chọn giới tính: 0. Nam | 1. Nữ | 2. Khác");
            while (true)
            {
                int val = EnterInt("Nhập lựa chọn (0-2): ");
                if (Enum.IsDefined(typeof(Gender), val)) return (Gender)val;
                Console.WriteLine(">> Lỗi: Giới tính không hợp lệ!");
            }
        }
        // Hàm nhập Ngày tháng chuẩn xác
        static bool TryGetDate(string prompt, out DateTime result)
        {
            Console.Write(prompt);
            return DateTime.TryParseExact(Console.ReadLine(), "dd/MM/yyyy", null, DateTimeStyles.None, out result);
        }
        // LOGIC CHƯƠNG TRÌNH
        static void InitRooms()
        {
            if (rooms.Count > 0) return; // Nếu đã load file thì thôi

            rooms.Add(new Room(101, 1, 200000, 2));
            rooms.Add(new Room(102, 1, 250000, 2));
            rooms.Add(new Room(201, 2, 300000, 4));
            rooms.Add(new Room(202, 2, 350000, 4));
            rooms.Add(new Room(301, 3, 400000, 2));
            rooms.Add(new Room(302, 3, 450000, 2));
            rooms.Add(new Room(401, 4, 500000, 4));
            rooms.Add(new Room(402, 4, 550000, 4));

        }
         static void ShowMapMatrix()
        {
            Console.WriteLine("\n--- SƠ ĐỒ KHÁCH SẠN ---");

            for (int floorIndex = 0; floorIndex < 4; floorIndex++)
            {
                Console.Write($"Tầng {floorIndex + 1}: ");

                for (int roomIndex = 0; roomIndex < 2; roomIndex++)
                {
                    int roomNum = roomMap[floorIndex, roomIndex];
                    Console.Write(roomNum == 0 ? "[ --- ] " : $"[ {roomNum} ] ");
                }
                Console.WriteLine();
            }
        }

        static void ShowRooms()
        {
            Console.WriteLine("\n--- DANH SÁCH PHÒNG ---");
            foreach (var r in rooms) Console.WriteLine(r.ToString());
        }
        static void BookRoom()
        {
            Console.WriteLine("\n--- ĐẶT PHÒNG ---");

            if (!TryGetDate("Ngày nhận (dd/MM/yyyy): ", out DateTime checkIn) ||
                !TryGetDate("Ngày trả (dd/MM/yyyy): ", out DateTime checkOut))
            {
                LogError("Ngày tháng sai định dạng!");
                return;
            }

            if (checkOut <= checkIn) { LogError("Ngày trả phải sau ngày nhận!"); return; }

            // Logic tìm phòng trống
            Console.WriteLine("Các phòng trống:");
            bool foundRoom = false;
            foreach (var r in rooms)
            {
                bool isBusy = false;
                foreach (var b in bookings)
                {
                    if (b.RoomNo == r.SoPhong)
                    {
                        if (checkIn < b.CheckOut && checkOut > b.CheckIn)
                        {
                            isBusy = true; break;
                        }
                    }
                }
                if (!isBusy)
                {
                    Console.WriteLine(r.ToString());
                    foundRoom = true;
                } }
            if (!foundRoom) { Console.WriteLine("Hết phòng trong khoảng thời gian này!"); return; }

            int roomNo = EnterInt("Nhập số phòng muốn đặt: ");
            var room = rooms.FirstOrDefault(r => r.SoPhong == roomNo);
            // Validate xem phòng nhập vào có tồn tại và trống không
            if (room == null) { LogError("Phòng không tồn tại!"); return; }
            // Check lại 1 lần nữa cho chắc
            bool doubleCheckBusy = bookings.Any(b => b.RoomNo == roomNo && checkIn < b.CheckOut && checkOut > b.CheckIn);
            if (doubleCheckBusy) { LogError("Phòng này đã có người đặt rồi!"); return; }
            string ten = EnterString("Tên khách: ");
            string cccd = EnterString("CCCD: ");
            string sdt = EnterString("SĐT: ");
            Gender gt = EnterGender();
            int custId = AddCustomer(ten, cccd, sdt, gt);
            int days = (checkOut - checkIn).Days;
            decimal total = room.Gia * days;
            int id = bookings.Count > 0 ? bookings.Max(b => b.Id) + 1 : 1;
            bookings.Add(new Booking(id, roomNo, custId, checkIn, checkOut, total));
            Console.WriteLine("=> Đặt thành công!");
        }
        // QUẢN LÝ DỮ LIỆU (SỬA/XÓA) - GIẢI QUYẾT VẤN ĐỀ 2
        static void ManageData()
        {
            while (true)
            {
                Console.WriteLine("\n--- QUẢN LÝ DỮ LIỆU ---");
                Console.WriteLine("1. Chỉnh sửa thông tin khách hàng");
                Console.WriteLine("2. Chỉnh sửa thông tin đặt phòng");
                Console.WriteLine("3. Quay lại menu chính");

                int choice = EnterInt("Chọn (1-3): ");

                if (choice == 3) break;

                if (choice == 1)
                {
                    // --- SỬA THÔNG TIN KHÁCH HÀNG ---
                    string inputCCCD = EnterString("Nhập CCCD khách cần sửa: ");
                    // Vì Customer là Struct, ta phải tìm Index để sửa trong List
                   int index = LinearSearchCustomerByCCCD(customers, inputCCCD);
                    if (index == -1)
                    {
                        LogError("Không tìm thấy khách hàng!");
                    }
                    else
                    {
                        // Lấy bản sao ra
                        Customer c = customers[index];
                        Console.WriteLine($"Thông tin cũ: {c.Ten} - SĐT: {c.SDT}");

                        // Nhập thông tin mới
                        c.Ten = EnterString("Nhập tên mới: ");
                        c.SDT = EnterString("Nhập SĐT mới: ");
                        c.GioiTinh = EnterGender();

                        // Gán ngược lại vào list (Quan trọng với Struct)
                        customers[index] = c;
                        Console.WriteLine("=> Cập nhật thành công!");
                    }
                }
                else if (choice == 2)
    {
        Console.Write("Nhập CCCD khách hàng: ");
        string cccd = Console.ReadLine();
        var customer = customers.FirstOrDefault(c => c.CCCD == cccd);
        if (customer.Equals(default(Customer)))
        {
             Console.WriteLine("Không tìm thấy khách hàng.");
             return;
         }

        var customerBookings = bookings
            .Where(b => b.CustomerId == customer.Id)
            .ToList();

        if (customerBookings.Count == 0)
        {
            Console.WriteLine("Khách hàng này chưa đặt phòng nào.");
            return;
        }

        Console.WriteLine("\nDanh sách phòng đã đặt:");
        foreach (var b in customerBookings)
        {
            Console.WriteLine(
                $"Phòng {b.RoomNo} | {b.CheckIn:dd/MM/yyyy} - {b.CheckOut:dd/MM/yyyy}");
        }

        Console.Write("\nNhập số phòng muốn chỉnh sửa/hủy: ");
        int roomNumber = int.Parse(Console.ReadLine());

        var booking = customerBookings
            .FirstOrDefault(b => b.RoomNo == roomNumber);

        if (booking.RoomNo == 0)
        {
            Console.WriteLine("Không tìm thấy đặt phòng.");
            return;
        }

        Console.WriteLine("\n1. Thay đổi ngày nhận - trả phòng");
        Console.WriteLine("2. Hủy đặt phòng");
        Console.Write("Chọn: ");
        string opt = Console.ReadLine();

        if (opt == "1")
        {
            Console.Write("Ngày nhận mới (dd/MM/yyyy): ");
            DateTime newCheckIn = DateTime.ParseExact(
                Console.ReadLine(), "dd/MM/yyyy", CultureInfo.InvariantCulture);

            Console.Write("Ngày trả mới (dd/MM/yyyy): ");
            DateTime newCheckOut = DateTime.ParseExact(
                Console.ReadLine(), "dd/MM/yyyy", CultureInfo.InvariantCulture);

            booking.CheckIn = newCheckIn;
            booking.CheckOut = newCheckOut;

            Console.WriteLine(" Đã cập nhật ngày nhận – trả phòng.");
        }
        else if (opt == "2")
        {
            bookings.Remove(booking);
            Console.WriteLine(" Đã hủy đặt phòng.");
        }
        else
        {
            Console.WriteLine("Lựa chọn không hợp lệ.");
        }
    }
        }
        }
        static void ShowBookingsByDateAsc()
        {
            ShowBookingsByDateAsc(DateTime.Today);
        }
        static void ShowBookingsByDateAsc(DateTime fromDate)
        {
            Console.WriteLine("\n--- DANH SÁCH KHÁCH ĐANG / SẼ LƯU TRÚ ---");
             List<Booking> result = new List<Booking>();
             foreach (var b in bookings)
              {
                 if (b.CheckOut >= fromDate)
                 result.Add(b);
            }
             if (result.Count == 0)
              {
                 Console.WriteLine("Không có khách phù hợp.");
                 return;
              }
              //  Bubble Sort – sắp xếp theo ngày nhận tăng dần
               for (int i = 0; i < result.Count - 1; i++)
                for (int j = 0; j < result.Count - i - 1; j++)
                if (result[j].CheckIn > result[j + 1].CheckIn)
                 {
                     Booking temp = result[j];
                     result[j] = result[j + 1];
                     result[j + 1] = temp;
                 }
                Console.WriteLine("Ngày nhận - trả | Tên khách | Số phòng");
                foreach (var b in result)
                {
                    Customer c = customers.First(x => x.Id == b.CustomerId);
                     Console.WriteLine($"{b.CheckIn:dd/MM/yyyy} - {b.CheckOut:dd/MM/yyyy} | {c.Ten} | Phòng {b.RoomNo}");
                 }
            }
        static void SortBookingsByPrice()
        {
            Booking[] arr = bookings.ToArray();
            int n = arr.Length;
            for (int i = 0; i < n - 1; i++)
                for (int j = 0; j < n - i - 1; j++)
                    if (arr[j].Total < arr[j + 1].Total)
                    {
                        Booking temp = arr[j];
                        arr[j] = arr[j + 1];
                        arr[j + 1] = temp;
                    }

            Console.WriteLine("\n--- TOP DOANH THU ---");
            foreach (var b in arr)
                Console.WriteLine($"Phòng {b.RoomNo} - Tổng tiền: {b.Total:N0} VNĐ");
        }

        static void Report()
        {
            Console.WriteLine($"\nTổng số đơn đặt: {bookings.Count}");
            Console.WriteLine($"Tổng doanh thu toàn bộ: {bookings.Sum(b => b.Total):N0} VNĐ");
        }

        static int AddCustomer(string ten, string cccd, string sdt, Gender gt)
        {
            var existing = customers.FirstOrDefault(c => c.CCCD == cccd);
            // Struct check default hơi khác class chút
            if (existing.CCCD != null) return existing.Id;

            int id = customers.Count > 0 ? customers.Max(c => c.Id) + 1 : 1;
            customers.Add(new Customer { Id = id, Ten = ten, CCCD = cccd, SDT = sdt, GioiTinh = gt });
            return id;
        }

        static void LogError(string msg, bool beep = true)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("LỖI: " + msg);
            Console.ResetColor();
            if (beep) Console.Beep();
        }

        static void SaveData()
        {
            try
            {
                if (File.Exists(BOOK_FILE)) File.Copy(BOOK_FILE, BOOK_FILE + ".bak", true);

                File.WriteAllLines(ROOMS_FILE, rooms.Select(r => $"{r.SoPhong},{r.Tang},{r.Gia},{r.SucChua}"));
                File.WriteAllLines(CUST_FILE, customers.Select(c => $"{c.Id}|{c.Ten}|{c.CCCD}|{c.SDT}|{(int)c.GioiTinh}"));
                File.WriteAllLines(BOOK_FILE, bookings.Select(b => $"{b.Id},{b.RoomNo},{b.CustomerId},{b.CheckIn},{b.CheckOut},{b.Total}"));

                Console.WriteLine("Đã lưu dữ liệu thành công!");
            }
            catch (Exception ex)
            {
                LogError("Không thể ghi file: " + ex.Message);
            }
        }
        static void LoadData()
        {
            try
            {
                /*  if (File.Exists(ROOMS_FILE))
                  {
                      rooms.Clear();
                      string[] lines = File.ReadAllLines(ROOMS_FILE);
                      foreach (string line in lines)
                      {
                          if (string.IsNullOrWhiteSpace(line)) continue;
                          string[] p = line.Split(',');
                          rooms.Add(new Room(int.Parse(p[0]), int.Parse(p[1]), decimal.Parse(p[2]), int.Parse(p[3])));
                      }
                  }*/
                if (File.Exists(ROOMS_FILE))
                {
                    var lines = File.ReadAllLines(ROOMS_FILE)
                                    .Where(l => !string.IsNullOrWhiteSpace(l))
                                    .ToList();

                    if (lines.Count > 0) // CHỈ load khi file có dữ liệu
                    {
                        rooms.Clear();
                        foreach (string line in lines)
                        {
                            var p = line.Split(',');
                            rooms.Add(new Room(
                                int.Parse(p[0]),
                                int.Parse(p[1]),
                                decimal.Parse(p[2]),
                                int.Parse(p[3])
                            ));
                        }
                    }
                }

                if (File.Exists(CUST_FILE))
                {
                    customers.Clear();
                    string[] lines = File.ReadAllLines(CUST_FILE);
                    foreach (string line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        string[] p = line.Split('|');
                        Customer c = new Customer();
                        c.Id = int.Parse(p[0]);
                        c.Ten = p[1];
                        c.CCCD = p[2];
                        c.SDT = p[3];
                        c.GioiTinh = (Gender)int.Parse(p[4]);
                        customers.Add(c);
                    }
                }

                if (File.Exists(BOOK_FILE))
                {
                    bookings.Clear();
                    string[] lines = File.ReadAllLines(BOOK_FILE);
                    foreach (string line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        string[] p = line.Split(',');
                        bookings.Add(new Booking(int.Parse(p[0]), int.Parse(p[1]), int.Parse(p[2]), DateTime.Parse(p[3]), DateTime.Parse(p[4]), decimal.Parse(p[5])));
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("Lỗi đọc file dữ liệu: " + ex.Message);
            }
        }
        static void BuildRoomMap()
        {
            roomMap = new int[5, 3]; // reset map

            foreach (var r in rooms)
            {
                int floorIndex = r.Tang - 1;
                int roomIndex = (r.SoPhong % 100) - 1;

                if (floorIndex >= 0 && floorIndex < 5 &&
                    roomIndex >= 0 && roomIndex < 3)
                {
                    roomMap[floorIndex, roomIndex] = r.SoPhong;
                }
            }
        }
        static int LinearSearchCustomerByCCCD(List<Customer> list, string cccd)
        {
            for (int i = 0; i < list.Count; i++)
            {
                 if (list[i].CCCD == cccd)
                 return i;
            }
            return -1;
        }
    }
}