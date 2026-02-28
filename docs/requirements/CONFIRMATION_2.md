# Round 2 — Câu hỏi làm rõ nghiệp vụ

---

# A. Quy trình Thanh toán & Hóa đơn

## 1. Khi một bệnh nhân sử dụng nhiều dịch vụ trong cùng một lần khám (khám + kính + thuốc), việc thanh toán được xử lý như thế nào?

* [x] Một hóa đơn gộp chung (khám + kính + thuốc + điều trị)
* [ ] Hóa đơn tách riêng theo từng phân hệ (HIS / kính / nhà thuốc)
* [ ] Tùy từng trường hợp, nhân viên quyết định gộp hoặc tách
* [ ] Khác:

### Giải thích

* Trải nghiệm bệnh nhân đơn giản
* Thuận lợi đối soát
* Dễ tích hợp MISA

Tuy nhiên hệ thống vẫn cần lưu doanh thu theo từng phân hệ nội bộ.

---

## 2. Phòng khám có chính sách khách hàng thân thiết hoặc ưu đãi cho bệnh nhân quay lại không?

* [x] Chương trình tích điểm / thẻ thành viên
* [ ] Giảm giá cho bệnh nhân quay lại (% hoặc số tiền cố định)
* [ ] Hiện chưa có, có thể bổ sung sau
* [ ] Khác: ___

### Mô hình Thẻ thành viên (Membership Tier)

**Điều kiện đạt VIP:**

* Tổng chi tiêu 12 tháng đạt ngưỡng nhất định
  **hoặc**
* Hoàn thành liệu trình điều trị chuyên sâu

**Thời hạn VIP:** 12 tháng

### Quyền lợi

* Giảm 10% phí khám tái khám
* Người thân (tối đa 2 người) được giảm 10% phí khám
* Giảm 5–7% phí dịch vụ (cấu hình theo nhóm dịch vụ)

Không áp dụng cho thuốc kê đơn hoặc các sản phẩm có giá cố định.

### Yêu cầu kỹ thuật

* Tự động tính điều kiện nâng hạng
* Có cấu hình thời hạn VIP
* Có lịch sử thay đổi hạng
* Cho phép cấu hình % giảm theo từng loại dịch vụ

---

## 3. Bảng giá dịch vụ có thay đổi theo thời gian không? Ai có quyền cập nhật giá?

* [x] Chỉ quản lý / chủ phòng khám
* [ ] Bác sĩ đề xuất, quản lý phê duyệt
* [ ] Khác: ___

Yêu cầu:

* Cần lưu lịch sử thay đổi giá (audit log)

---

# B. Quy trình Trung tâm kính

## 4. Khi bệnh nhân đặt kính, có yêu cầu đặt cọc không?

* [ ] Có, bắt buộc đặt cọc (% hoặc số tiền cố định)
* [ ] Tùy từng trường hợp
* [ ] Không yêu cầu đặt cọc
* [x] Khác: Bệnh nhân thanh toán trước khi mài lắp kính, không thu tiền cọc

---

## 5. Thời gian trung bình cắt/lắp tròng kính là bao lâu?

* [ ] Trong ngày (< 4 giờ)
* [x] 1–3 ngày
* [ ] 3–7 ngày (gia công bên ngoài)
* [ ] Khác: ___

### Hệ thống cần có trạng thái:

* Đã đặt
* Đang thực hiện kỹ thuật
* Đã nhận hàng
* Sẵn sàng giao
* Đã giao

---

## 6. Kính sau khi hoàn thiện được giao cho bệnh nhân bằng cách nào?

* [ ] Khách đến nhận tại phòng khám
* [ ] Giao tận nhà (ship)
* [x] Cả hai hình thức
* [ ] Khác: ___

---

## 7. Chính sách bảo hành kính cụ thể như thế nào?

**Thời gian bảo hành:**

* Gọng: 12 tháng
* Tròng: 12 tháng

**Bảo hành bao gồm:**

* [x] Thay mới hoàn toàn
* [x] Sửa chữa miễn phí
* [x] Giảm giá khi thay thế
* [ ] Khác: ___

Lưu ý:
Tùy mức độ, nguyên nhân và chứng từ đính kèm để quyết định bảo hành.

---

# C. Quản lý Liệu trình Điều trị

## 8. Nếu bệnh nhân muốn hủy liệu trình giữa chừng, chính sách hoàn tiền như thế nào?

* [ ] Hoàn 100% các buổi chưa sử dụng
* [x] Hoàn tiền có trừ phí (ví dụ 10–20%)
* [ ] Không hoàn tiền
* [ ] Tùy từng trường hợp do quản lý quyết định
* [ ] Khác: ___

Yêu cầu:

* Cần cấu hình linh hoạt theo quyết định quản lý

---

## 9. Bệnh nhân có thể chuyển đổi gói điều trị giữa chừng không? (ví dụ: từ IPL sang LLLT)

* [ ] Có, tự do chuyển đổi
* [x] Có, nhưng cần bác sĩ phê duyệt
* [ ] Không, phải hoàn thành gói hiện tại
* [ ] Khác: ___

---

## 10. Có quy định khoảng cách tối thiểu giữa các buổi điều trị không?

* [x] Có

### Khoảng cách tối thiểu:

* IPL: 2–4 tuần

* LLLT: 1–2 tuần

* Chăm sóc mi: 1–2 tuần

* [ ] Bác sĩ quyết định theo từng trường hợp

* [ ] Khác: ___

---

# D. Nhà thuốc

## 11. Đơn thuốc có thời hạn hiệu lực không?

* [ ] 3 ngày
* [ ] 5 ngày
* [x] 7 ngày
* [ ] Khác: ___

Ghi chú:
Theo quy định Bộ Y tế Việt Nam, đơn thuốc có thời hạn hiệu lực.
Hệ thống cần cảnh báo khi đơn thuốc hết hạn.

---

## 12. Có thiết lập mức tồn kho tối thiểu (min stock) cho từng thuốc không?

* [x] Có, thiết lập cho từng thuốc
* [ ] Có, thiết lập theo nhóm thuốc
* [ ] Chưa có, cần cấu hình
* [ ] Khác: ___

---

## 13. Quy trình nhập kho thuốc được thực hiện như thế nào?

* [ ] Nhập từ hóa đơn nhà cung cấp (scan / nhập tay)
* [ ] Import từ file Excel
* [x] Cả hai
* [ ] Khác: ___

---

# E. Lens điều trị (Ortho-K / Contact Lens)

## 14. Quy trình đặt và theo dõi lens điều trị (Ortho-K) như thế nào?

* [x] Bác sĩ kê → đặt từ nhà cung cấp → nhận lens → hẹn bệnh nhân thử/lắp
* [x] Có sẵn lens trial để thử trước khi đặt chính thức
* [ ] Khác: ___

---

## 15. Sau khi bệnh nhân bắt đầu sử dụng Ortho-K, lịch tái khám theo protocol như thế nào?

* [x] 1 ngày → 1 tuần → 1 tháng → 3 tháng → 6 tháng
* [ ] Bác sĩ quyết định theo từng trường hợp
* [ ] Khác: ___

Yêu cầu:

* Hệ thống cần tự động tạo lịch nhắc theo protocol điều trị

---

# F. Nhân sự & Ca làm việc

## 16. Nhân viên có làm việc theo ca không? Có cần theo dõi doanh thu theo từng ca không?

* [x] Có, làm theo ca và theo dõi doanh thu theo ca
* [ ] Có làm theo ca nhưng không cần theo dõi doanh thu theo ca
* [ ] Không chia ca (giờ hành chính cố định)
* [ ] Khác: ___

---

## 17. Dự kiến số lượng nhân sự khi triển khai?

* Bác sĩ: 2 người
* Kỹ thuật viên: 2 người
* Điều dưỡng: 1 người
* Thu ngân: 1 người
* Nhân viên kính: 1 người
* Quản lý: 1 người

Ghi chú:
Thông tin này giúp thiết kế phân quyền và cấu hình ban đầu hệ thống.

---

# G. Dữ liệu & Tích hợp

## 18. Có dữ liệu bệnh nhân hiện tại cần chuyển sang hệ thống mới không?

* [ ] Có (từ Excel / phần mềm cũ)
* [x] Không, bắt đầu từ đầu
* [ ] Khác: ___

---

## 19. Mức độ tích hợp với phần mềm kế toán MISA mong muốn?

* [ ] Chỉ xuất báo cáo, nhập thủ công vào MISA
* [ ] Đồng bộ tự động doanh thu/chi phí sang MISA
* [ ] Khác: ___

☑ Giai đoạn 1: Xuất báo cáo → nhập MISA thủ công
☑ Giai đoạn 2: Xem xét đồng bộ tự động

---

## 20. Giao diện hệ thống mong muốn sử dụng ngôn ngữ nào?

* [ ] Chỉ tiếng Việt
* [x] Song ngữ Việt – Anh
* [ ] Song ngữ Việt – Nhật
* [ ] Khác: ___
