Dưới đây là toàn bộ nội dung file đã được chuyển đổi đầy đủ sang **Markdown**, giữ nguyên cấu trúc và các lựa chọn đã đánh dấu:

---

# Vòng 4 — Làm rõ Logic Nghiệp vụ

Ba vòng trước đã rất đầy đủ.
Đây là những câu hỏi còn lại ảnh hưởng trực tiếp đến thiết kế hệ thống.

---

# Quản lý Bệnh nhân

## Hệ thống sử dụng mã bệnh nhân như thế nào?

* [ ] Tự động tạo số thứ tự (GK-0001, GK-0002...)
* [ ] Dùng số điện thoại làm mã định danh chính
* [x] Theo định dạng năm + số thứ tự (GK-2026-0001)
* [ ] Khác: ___

### Lý do

* Dễ quản lý theo năm
* Không phụ thuộc SĐT (SĐT có thể đổi)
* Thuận tiện export nghiên cứu
* Chuyên nghiệp hơn dùng SĐT làm ID

---

## Khi đăng ký bệnh nhân mới, những thông tin nào bắt buộc nhập?

* [ ] Họ tên + Số điện thoại (tối thiểu)
* [x] Họ tên + Số điện thoại + Ngày sinh + Giới tính
* [ ] Họ tên + Số điện thoại + Ngày sinh + Giới tính + Địa chỉ + CCCD/CMND
* [ ] Khác: ___

### Lưu ý

* Thông tin tối thiểu cho khách vãng lai mua thuốc: **Họ tên + SĐT**
* Câu hỏi này áp dụng cho **bệnh nhân khám bệnh**

Hệ thống cần cho phép cấu hình bổ sung bắt buộc **Địa chỉ + CCCD** trong các trường hợp:

* Xuất hồ sơ pháp lý
* Chuyển viện
* Đồng bộ dữ liệu theo yêu cầu Sở Y tế trong tương lai

---

## Bệnh nhân có thể khám bác sĩ khác nhau giữa các lần khám không?

* [x] Có — hệ thống lưu lịch sử bác sĩ khám theo từng lần
* [ ] Không — mỗi bệnh nhân chỉ gán 1 bác sĩ điều trị chính
* [ ] Khác: ___

---

# Đơn thuốc & Khám bệnh

## Đơn thuốc có cần chữ ký điện tử của bác sĩ không?

* [ ] Có — bác sĩ xác nhận điện tử trước khi in
* [x] Không — chỉ cần tên bác sĩ trên đơn
* [ ] Khác: ___

> Chữ ký điện tử có thể triển khai ở Phase 2 nếu kết nối EMR chính thức.

---

## Dữ liệu đo khúc xạ bao gồm những chỉ số nào? (Chọn tất cả)

* [x] SPH (Cầu)
* [x] CYL (Trụ)
* [x] AXIS (Trục)
* [x] ADD (Cộng)
* [x] PD (Khoảng cách đồng tử)
* [x] VA (Thị lực — có kính / không kính)
* [x] IOP (Nhãn áp)
* [x] Axial Length (cho Myopia Control)

> Axial Length là chỉ số quan trọng của Ganka — không được bỏ.

### Lý do

Xác định cấu trúc dữ liệu cho mẫu đo khúc xạ và đơn kính.

---

## Khi kê đơn thuốc, bác sĩ chọn từ danh mục thuốc có sẵn hay nhập tự do?

* [ ] Chọn từ danh mục thuốc trong hệ thống (liên kết kho)
* [ ] Nhập tên thuốc tự do (không liên kết kho)
* [x] Cả hai — chọn từ danh mục, có thể thêm thuốc ngoài danh mục
* [ ] Khác: ___

### Lý do

Liên kết đơn thuốc — kho thuốc để xác định lượng xuất kho tự động hay thủ công.

---

# Tài chính & Thanh toán

## Nhân viên thu ngân có được phép giảm giá ngoài VIP không?

* [ ] Không — chỉ có giảm giá VIP tự động
* [ ] Có — thu ngân được giảm trong giới hạn (ví dụ: tối đa 10%)
* [x] Có — nhưng cần quản lý phê duyệt
* [ ] Khác: ___

---

## Khi bệnh nhân hoàn tiền (hủy liệu trình, bảo hành), quy trình như thế nào?

* [x] Chỉ quản lý/chủ phòng khám phê duyệt hoàn tiền
* [ ] Thu ngân có thể tự thực hiện hoàn tiền
* [ ] Khác: ___

---

## Thanh toán trả góp liệu trình (50% / 50%) — đợt thanh toán thứ 2 vào lúc nào?

* [ ] Trước buổi thứ 3 (sau 2 buổi đầu)
* [x] Trước buổi giữa liệu trình
* [ ] Thu ngân/quản lý quyết định linh hoạt
* [ ] Khác: ___

### Ví dụ

* Gói 5 buổi → Trước buổi 3 phải thanh toán đủ
* Gói 3 buổi → Trước buổi 2

---

# Vật tư Tiêu hao & Kho

## Vật tư tiêu hao điều trị (gel IPL, miếng che mắt, v.v.) lấy từ kho nào?

* [ ] Từ kho nhà thuốc (chung với thuốc)
* [x] Kho vật tư riêng (tách biệt với nhà thuốc)
* [ ] Khác: ___

### Lý do

Xác định có cần thêm 1 kho riêng cho vật tư hay dùng chung với nhà thuốc.

---

# Đặt lịch hẹn

## Bệnh nhân có tự đặt lịch hẹn qua website/Zalo không, hay chỉ nhân viên đặt?

* [ ] Chỉ nhân viên đặt lịch (bệnh nhân gọi điện hoặc đến trực tiếp)
* [ ] Bệnh nhân tự đặt qua website/Zalo — nhân viên xác nhận
* [x] Cả hai
* [ ] Khác: ___

### Lý do

Nếu bệnh nhân tự đặt lịch, cần thiết kế giao diện booking và luồng xác nhận.

Lưu ý:
Module bán hàng online đã defer, nhưng tính năng đặt lịch có thể cần cho v1.

---

# Báo cáo & Xuất dữ liệu

## Xuất dữ liệu phục vụ nghiên cứu — cụ thể là dữ liệu gì?

* [ ] Dữ liệu khám (OSDI, TBUT, Schirmer... theo thời gian) — ẩn danh
* [ ] Dữ liệu điều trị (kết quả liệu trình theo protocol)
* [ ] Dữ liệu khúc xạ (tiến triển cận thị theo nhóm tuổi)
* [x] Tất cả những mục trên
* [ ] Khác: ___

### Lý do

Xác định định dạng xuất và tính năng ẩn danh hóa dữ liệu.

---