# Vòng 3 — Làm rõ Logic Nghiệp vụ

---

# A. Nhà thuốc — Bán hàng trực tiếp

## Khi một khách không phải bệnh nhân đến mua thuốc không kê đơn (nhỏ mắt, Omega-3, v.v.), hệ thống nên xử lý như thế nào?

* [x] Đăng ký họ như một bệnh nhân trong hệ thống (tối thiểu: tên + số điện thoại)
* [ ] Bán hàng POS nhanh — không cần hồ sơ bệnh nhân (chỉ cần sản phẩm + thanh toán)
* [ ] Khác: ___

### Lý do đặt câu hỏi

Quyết định xem POS nhà thuốc có cần tra cứu bệnh nhân đầy đủ hay có thể hoạt động độc lập cho khách mua lẻ.

### Lý do lựa chọn

* Giúp quản lý lịch sử mua sản phẩm khô mắt / Omega 3
* Phục vụ loyalty & VIP
* Thuận tiện chăm sóc khách hàng sau này

Không cần tạo hồ sơ bệnh án đầy đủ, chỉ cần hồ sơ khách hàng tối thiểu.

---

# B. Lịch hẹn

## Giờ hoạt động của phòng khám

* Thứ 3 – Thứ 6: 13h00–20h00
* Thứ 7, Chủ nhật: 8h00–12h00
* Có thể mở rộng Thứ 7, Chủ nhật chiều: 13h00–17h00
* Nghỉ Thứ 2 hàng tuần

---

## Thời lượng mỗi loại lịch hẹn (ước tính)

* Khách mới (lần đầu): 30 phút
* Tái khám: 20 phút
* Buổi điều trị (IPL/LLLT): 30–45 phút
* Đo Ortho-K: 60–90 phút

---

## Một bác sĩ có thể khám bao nhiêu bệnh nhân mỗi khung giờ?

* [x] 1 bệnh nhân mỗi khung giờ (xếp lịch chặt chẽ)
* [ ] Nhiều bệnh nhân mỗi khung giờ (cho phép overbooking)
* [ ] Không dùng khung giờ — xếp hàng theo thứ tự đến
* [ ] Khác: ___

---

# C. Phòng chờ / Quản lý hàng đợi

## Bệnh nhân được quản lý trong phòng chờ như thế nào?

* [ ] Hệ thống xếp hàng điện tử với số thứ tự và màn hình hiển thị
* [x] Nhân viên gọi tên bệnh nhân (không cần hệ thống xếp hàng điện tử cho phiên bản v1)
* [ ] Khác: ___

### Lý do đặt câu hỏi

Hệ thống xếp hàng điện tử là một module riêng. Nếu chưa cần cho v1, việc triển khai sẽ đơn giản hơn.

### Ghi chú

V1 không cần và thực tế phòng khám không cần phải có số điện tử.
Phòng khám theo mô hình boutique, gọi tên bệnh nhân theo kiểu “gia đình”.

### Lý do

* Quy mô nhỏ
* Trải nghiệm boutique
* Không cần màn hình số thứ tự

---

# D. Thanh toán gói điều trị

## Đối với giá gói (ví dụ: gói IPL 6 buổi), bệnh nhân thanh toán khi nào?

* [x] Thanh toán toàn bộ trước buổi đầu tiên
* [x] Có thể thanh toán 2 đợt (ví dụ: 50% trước, 50% sau 2 buổi liệu trình)
* [ ] Linh hoạt — nhân viên quyết định theo từng bệnh nhân
* [ ] Khác: ___

---

# E. Yêu cầu in ấn

## Những tài liệu nào cần in từ hệ thống? (Chọn tất cả các mục áp dụng)

* [x] Đơn thuốc
* [x] Đơn kính (Rx cho bộ phận kính)
* [x] Hóa đơn / Biên lai
* [ ] Giấy khám sức khỏe
* [x] Giấy chuyển viện
* [x] Phiếu đồng ý điều trị
* [ ] Thẻ / phiếu nhắc lịch hẹn
* [x] Nhãn thuốc nhà thuốc
* [ ] Khác: ___

### Lý do đặt câu hỏi

Thiết kế bố cục in và phạm vi hệ thống mẫu in phụ thuộc vào yêu cầu này.

---

# F. An toàn lâm sàng

## Hệ thống có cần cảnh báo dị ứng thuốc / tương tác thuốc không?

* [x] Có — cảnh báo khi kê thuốc mà bệnh nhân bị dị ứng
* [ ] Có — kiểm tra đầy đủ tương tác giữa các thuốc
* [ ] Chỉ cần trường thông tin dị ứng cơ bản (không kiểm tra tự động cho v1)
* [ ] Khác: ___

---

## Hệ thống có cần lưu thông tin dị ứng và tiền sử bệnh của bệnh nhân không?

* [x] Có — danh sách dị ứng dạng cấu trúc với cảnh báo khi kê đơn
* [ ] Có — ghi chú dị ứng dạng văn bản tự do trong hồ sơ bệnh nhân
* [ ] Không cần cho phiên bản v1
* [ ] Khác: ___

---

# G. Giao tiếp với bệnh nhân

## 10. Ngoài nhắc lịch hẹn, hệ thống nên gửi thêm những tin nhắn tự động nào qua Zalo?

* [x] Tóm tắt sau khám / lời cảm ơn
* [x] Nhắc buổi trong liệu trình điều trị
  *(Ví dụ: “Buổi IPL tiếp theo của bạn đã đến hạn”)*
* [x] Thông báo kính đã sẵn sàng để nhận
* [ ] Chúc mừng sinh nhật / ngày lễ
* [ ] Không cần thêm ngoài nhắc lịch hẹn cho v1
* [ ] Khác: ___

---