Tách riêng movement ra, dùng static cho các biến nên khi thay đổi thì tất cả đều ảnh hưởng, tạo movement riêng cho từng quái ( cách này hơi đần nhưng chưa tìm ra giải pháp) (V)
Trong GameManagement cần thêm script để quản lý tài nguyên, quản lý quái, quản lý item, quản lý skill, quản lý level, quản lý game (~)
Cần thêm script để object con lấy sorting layer theo parent (V)
Sửa lại genetic phân phối tài nguyên, hiện tại đang coi các ingridient là tiêu tốn chứ không phải là tài nguyên cung cấp cho người chơi (~)
sửa lại Linear Regression đưa ra dự đoán về tài nguyên cần spawn cho level tiếp theo (V ?)
chỉ cần cân bằng hươu sói cho việc spawn còn đâu không cần tracking ecosystem (?)
PHẦN GA PHÂN BỐ TÀI NGUYÊN ĐANG SAI, CẦN XEM LẠI PENATY (V ?)	

Phần đọc dữ liệu để spawn object đang bị chạy trước phần ghi dữ liệu lại vào file json(V) nguyên nhân do dùng cách gắn file lên inspector(luôn đọc dữ liệu cũ không update theo frame) -> chuyển sang dùng file path để đọc ghi (V)

Thêm inventory cho player (~)

thêm phần load save xong mới đưa ra quyết định sử dụng spawn nào (GA hay Load số lượng từ save) 

bảng cấp độ (Level Table) trong ScriptableObject (?)

tìm giải pháp để truyền atkRanger từ stats vào BehaviorTree (callback function || switch / if) (V)

Phần target default target của enemy đang bị lỗi, fix ? (X)

Phần CheckInRange bị lỗi tính khoảng cách từ privot, đề xuất sửa chỉ tính khoảng cách X (X)

Bug Script DealingDmg gameobject được bật lên bởi nguyên nhân không rõ (Remove)

Khớp attack và animation:
  - khớp điều kiện tấn công và sử dụng skill (V)
  - khớp atkspd và animation (V)

Phần knockback chưa hoạt động (~)
