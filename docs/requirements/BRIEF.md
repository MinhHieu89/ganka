Dưới đây là toàn bộ nội dung file đã được chuyển đổi đầy đủ sang **Markdown**, giữ nguyên cấu trúc và ý nghĩa:

---

# HỆ THỐNG QUẢN LÝ PK GANKA28

---

# I. MỤC TIÊU TRIỂN KHAI

## 1. Đáp ứng yêu cầu pháp lý Việt Nam

Hệ thống phải:

* Kết nối dữ liệu với Sở Y tế theo quy định hiện hành và sẵn sàng đáp ứng yêu cầu trước ngày **31/12/2026**,nên cân có cần mã ICD10.
* Sẵn sàng kết nối cổng BHYT (nếu phòng khám triển khai).
* Lưu trữ hồ sơ bệnh án tối thiểu **10 năm**.
* Đảm bảo bảo mật dữ liệu theo tiêu chuẩn an toàn thông tin y tế:

  * Phân quyền
  * Ghi log truy cập
  * Backup

---

## 2. Quản lý tổng thể hoạt động phòng khám

Hệ thống phải quản lý đồng bộ:

* Khám chữa bệnh chuyên khoa mắt (bệnh mạn tính dài hạn)
* Kho thuốc
* Trung tâm kính (gọng – tròng – lens)
* Liệu trình điều trị (IPL, LLLT, kiểm soát cận thị…)
* Bán hàng online

Có khả năng mở rộng thành chuỗi phòng khám.

---

# II. PHẠM VI YÊU CẦU PHÂN HỆ

---

# 1️⃣ PHÂN HỆ QUẢN LÝ KHÁM CHỮA BỆNH (HIS)

## 1.1 Quản lý hồ sơ bệnh án

### Hệ thống phải:

* Tạo bệnh án điện tử theo từng lần khám.
* Cho phép tạo nhiều template bệnh án riêng biệt theo từng nhóm bệnh.
* Lưu trữ tối thiểu 10 năm.
* Tìm kiếm hồ sơ bệnh nhân trong thời gian ≤ 3 giây.

---

### Upload & quản lý hình ảnh

Phải hỗ trợ:

* Ảnh nhuộm Fluorescein
* Meibography
* Specular microscopy
* Topography
* OCT (nếu có)
* Lưu video (ví dụ: tuyến lệ)

Phải cho phép:

* So sánh song song 2 lần khám (side-by-side).
* Phóng to hình ảnh ≥ 200% không vỡ ảnh.
* Gắn hình ảnh đúng lần khám.

---

### Gửi kết quả & nhắc lịch

* Gửi link kết quả khám và đơn thuốc qua Zalo/Email có bảo mật.
* Nhắc lịch tái khám tự động qua SMS/Zalo.
* Không xảy ra trùng lịch (double booking).

---

## 1.2 Quản lý bệnh mắt mạn tính

Hệ thống phải hỗ trợ mô hình quản lý bệnh dài hạn (chronic care).

---

### 1.2.1 Bệnh lý khô mắt

Phải có template riêng cho **Dry Eye**.

Bắt buộc nhập:

* Kết quả bộ câu hỏi sàng lọc (OSDI hoặc tương đương)
* TBUT
* Schirmer
* Meibomian grading
* Tear meniscus
* Staining score

Phải:

* Vẽ biểu đồ OSDI theo thời gian.
* So sánh TBUT giữa các lần khám.
* Lưu hình ảnh từ sinh hiển vi.
* Xuất báo cáo tiến triển điều trị.

---

### 1.2.2 Bệnh lý cận thị (Kiểm soát cận thị)

Bắt buộc nhập:

* Chiều dài trục nhãn cầu (Axial length)
* Độ khúc xạ
* Tuổi
* Phương pháp điều trị (Atropine / Lens / Kính / Kết hợp)

Phải:

* Vẽ biểu đồ axial length theo năm.
* Cảnh báo tiến triển nhanh (ngưỡng cấu hình được).
* So sánh độ khúc xạ giữa các năm.
* Xuất báo cáo tiến triển cho phụ huynh.

---

### 1.2.3 Mở rộng bệnh mạn tính khác

Hệ thống phải mở rộng được template cho:

* Ghép giác mạc
* Glocom
* Đái tháo đường
* Keratoconus

Không cần viết lại hệ thống khi thêm template mới.

---

## 1.3 Kết nối thiết bị

Hệ thống phải có khả năng tích hợp:

* Máy sinh hiển vi
* Máy OCT
* Máy siêu âm mắt
* Máy bản đồ giác mạc
* Máy đếm tế bào nội mô
* Máy đo axial length
* Máy IPL
* Máy LLLT

Yêu cầu:

* Tự động import chỉ số và hình ảnh.
* Không yêu cầu nhập lại bằng tay toàn bộ dữ liệu.
* Có API mở để tích hợp thiết bị trong tương lai.
* Thời gian đồng bộ dữ liệu ≤ 30 giây sau khi chụp máy.

---

# 2️⃣ PHÂN HỆ QUẢN LÝ LIỆU TRÌNH ĐIỀU TRỊ

Hệ thống phải:

* Tạo gói IPL 1–6 buổi linh hoạt.
* Tạo gói LLLT 1–6 buổi.
* Tạo gói chăm sóc mi 1–6 buổi.
* Cho phép chỉnh sửa phác đồ giữa liệu trình.
* Theo dõi số buổi còn lại.
* Tự động chuyển trạng thái “Completed” khi đủ buổi.
* Ghi nhận OSDI theo từng buổi điều trị.
* Nhắc lịch tự động.

---

# 3️⃣ PHÂN HỆ QUẢN LÝ NHÀ THUỐC

Phải:

* Quản lý nhập – xuất – tồn.
* Theo dõi hạn dùng.
* Cảnh báo thuốc gần hết hạn.
* Kết nối hóa đơn điện tử.
* Sẵn sàng kết nối BHYT (nếu triển khai).
* Kiểm soát đơn thuốc theo quy định Bộ Y tế.

---

# 4️⃣ PHÂN HỆ TRUNG TÂM KÍNH

Kính là mảng doanh thu chiến lược.

Hệ thống phải:

## Quản lý độ kính theo bệnh nhân

* Lưu độ kính theo từng lần đo.
* So sánh độ giữa các năm.
* Lưu lịch sử thay tròng.

---

## Quản lý kho

* Quản lý gọng theo mã vạch.
* Quản lý tròng theo độ.
* Quản lý lens điều trị.
* Theo dõi lãi gộp từng sản phẩm.
* Quản lý combo gọng + tròng.
* Quản lý bảo hành.
* Kiểm kê bằng barcode.

---

## Phân tích

* Doanh thu theo nhân viên.
* Doanh thu theo nhóm sản phẩm.
* Doanh thu theo thương hiệu.
* Lãi gộp từng mảng.

---

# 5️⃣ PHÂN HỆ BÁN HÀNG ONLINE

Hệ thống phải:

* Website tích hợp đặt lịch khám.
* Đồng bộ lịch với Zalo OA và Facebook.

Bán:

* Kính
* Nước mắt nhân tạo
* Omega 3
* Sản phẩm hỗ trợ khô mắt

Thanh toán:

* QR
* Chuyển khoản
* Cổng thanh toán

Yêu cầu:

* Đồng bộ tồn kho real-time giữa online và offline (≤ 10 giây).
* Không cho phép bán khi hết hàng.
* Quản lý khuyến mãi.
* Quản lý bảo hành.

---

# 6️⃣ BÁO CÁO & TÀI CHÍNH

Hệ thống phải:

* Báo cáo doanh thu theo ngày/tháng/năm.
* Phân tích theo dịch vụ – thuốc – kính.
* Phân tích lãi gộp từng mảng.
* Dashboard trực quan.
* Xuất dữ liệu Excel/CSV phục vụ nghiên cứu.
* Xuất được dữ liệu ≥ 1000 bệnh nhân không lỗi file.

---

# III. YÊU CẦU KỸ THUẬT

* Cloud-based.
* Backup tự động hàng ngày.
* Phân quyền chi tiết theo vai trò:

  * BS
  * Điều dưỡng
  * Thu ngân
  * Quản lý
* Ghi log truy cập.
* Có mobile app hoặc giao diện mobile.
* API mở.
* Quyền sở hữu dữ liệu thuộc về Ganka28.

---

# IV. KẾT NỐI PHÁP LÝ BẮT BUỘC

Đơn vị cung cấp phải:

* Cam kết đáp ứng kết nối Sở Y tế trước 31/12/2026.
* Cung cấp lộ trình kỹ thuật cụ thể.
* Có kinh nghiệm triển khai phòng khám chuyên khoa.
* Có đội ngũ hỗ trợ tại Hà Nội.
* Cam kết SLA thời gian xử lý sự cố.

---

# V. ĐỀ NGHỊ BÁO GIÁ

Báo giá phải bao gồm:

* Chi phí cài đặt ban đầu.
* Chi phí customize chuyên khoa mắt.
* Phí duy trì hàng năm.
* Chi phí tích hợp thiết bị.
* Phí mở rộng khi thêm chi nhánh.
* Thời gian triển khai.
* Kế hoạch đào tạo.
* Demo hệ thống theo workflow thực tế của Ganka28.

---

# VI. ĐỊNH HƯỚNG TƯƠNG LAI

Hệ thống phải có khả năng mở rộng để:

* Tích hợp thêm chi nhánh.
* Tích hợp ngân hàng mô.
* Tích hợp module nghiên cứu lâm sàng.
* Tích hợp hệ thống quản lý chuỗi chuyên khoa mắt.
