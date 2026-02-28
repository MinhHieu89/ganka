# VÒNG 1 — Câu hỏi làm rõ logic nghiệp vụ cho dự án GANKA28

---

# A. Bối cảnh kinh doanh & vận hành

## A1. Mức ưu tiên & phân giai đoạn

### 1. Hệ thống con nào quan trọng nhất cho ngày ra mắt? (Xếp hạng ưu tiên)

* [1] HIS (Hệ thống thông tin bệnh viện)
* [1] Liệu trình điều trị
* [1] Nhà thuốc
* [1] Trung tâm kính
* [2] Bán hàng trực tuyến
* [1] Báo cáo

---

### 2. Bán hàng trực tuyến có phải yêu cầu cho ngày ra mắt không?

* [ ] Có, cần cho ngày ra mắt
* [x] Không, triển khai ở giai đoạn sau

---

### 3. Ganka28 hoạt động tại bao nhiêu cơ sở khi ra mắt?

* [x] Một cơ sở duy nhất
* [ ] Đa chi nhánh ngay từ đầu

**Lưu ý:**
→ Triển khai giai đoạn 2 (sau khi hệ lõi ổn định)

---

## A2. Doanh thu & thanh toán

### 4. Phòng khám sử dụng những phương thức thanh toán nào?

* [x] Tiền mặt
* [x] Chuyển khoản ngân hàng
* [x] QR code (VNPay, MoMo, ZaloPay...)
* [x] Thẻ (Visa/Mastercard)

---

### 5. Phòng khám đã kết nối BHYT chưa?

* [ ] Đã kết nối
* [x] Chưa, kế hoạch tương lai

---

### 6. Nhà cung cấp hóa đơn điện tử?

* [ ] VNPT
* [ ] Viettel
* [ ] FPT e-invoice
* [x] Khác: **MISA**

---

# B. HIS — Quy trình lâm sàng

## B1. Luồng khám bệnh nhân

### 7. Hành trình khám bệnh điển hình? (Đánh dấu các bước áp dụng)

* [x] Tiếp nhận/Đăng ký
* [ ] Phân loại/Sàng lọc
* [x] Đo khúc xạ/Thị lực (kỹ thuật viên)
* [x] Khám bác sĩ
* [x] Xét nghiệm/Chẩn đoán hình ảnh
* [x] Bác sĩ đọc kết quả
* [x] Kê đơn thuốc/đơn kính
* [x] Thu ngân
* [x] Nhà thuốc phát thuốc
* [x] Trung tâm kính tư vấn kính
* [x] Thu ngân
* [x] Chuyển phòng kỹ thuật mài lắp kính
* [x] Trả kính tại quầy thu ngân hoặc CPN

---

### 8. Hình thức tiếp nhận bệnh nhân?

* [ ] Khám trực tiếp (walk-in)
* [ ] Đặt lịch hẹn trước
* [x] Kết hợp cả hai

---

### 9. Ai thực hiện đo khúc xạ/kiểm tra thị lực?

* [ ] Bác sĩ tự thực hiện
* [ ] Kỹ thuật viên khúc xạ riêng
* [x] Cả hai tùy trường hợp

---

## B2. Bệnh án & mẫu biểu

### 10. Có thể cung cấp mẫu bệnh án giấy đang sử dụng không?

* [ ] Có, sẽ cung cấp
* [x] Không có sẵn / cùng xây dựng

---

### 11. Bộ câu hỏi đánh giá Khô mắt sử dụng?

* [x] OSDI
* [ ] DEQ-5
* [ ] SPEED
* [ ] Khác: ___

---

### 12. Thiết bị đo trục nhãn cầu đang sử dụng? (Kiểm soát Cận thị)

* [ ] IOLMaster
* [ ] Lenstar
* [ ] Myopia Master
* [x] Khác: **Dự kiến MYAH hoặc 1 máy OCT đời mới**

---

### 13. Mẫu bệnh mãn tính cần cho ngày ra mắt?

* [x] Khô mắt (Dry Eye)
* [ ] Kiểm soát Cận thị (Myopia Control)
* [ ] Glaucoma
* [ ] Giác mạc chóp (Keratoconus)
* [ ] Khác: ___

Ghi chú:
Những module sau có thể mở rộng sau khai trương.

---

## B3. Thông báo & tin nhắn

### 18. Phòng khám đã có Zalo OA chưa?

* [ ] Đã có
* [x] Chưa có

---

### 19. Thời điểm nhắc lịch hẹn?

* [x] 1 ngày trước
* [ ] 2 giờ trước
* [ ] Cả hai
* [ ] Khác: ___

---

### 20. Cần SMS làm phương thức dự phòng cho v1?

* [ ] Có, cần SMS dự phòng
* [x] Không, chỉ Zalo là đủ

---

# C. Liệu trình điều trị

### 21. Tính giá điều trị IPL/LLLT?

* [ ] Theo buổi
* [ ] Theo gói
* [x] Cả hai (buổi lẻ + gói)

---

### 22. Bệnh nhân có thể có nhiều liệu trình đang hoạt động cùng lúc?

* [x] Có
* [ ] Không

---

### 23. Ai được tạo/chỉnh sửa phác đồ điều trị?

* [x] Chỉ bác sĩ
* [ ] Bác sĩ và điều dưỡng

---

### 24. Theo dõi vật tư tiêu hao theo từng buổi điều trị?

* [x] Có (gel IPL, miếng che mắt, v.v.)
* [ ] Không cần

---

# D. Nhà thuốc

### 25. Nhà thuốc thuộc loại hình nào?

* [ ] Pháp nhân riêng biệt (nhà thuốc)
* [x] Thuộc phòng khám (tủ thuốc)

---

### 26. Nhà thuốc bán cho ai?

* [ ] Chỉ bệnh nhân phòng khám (theo đơn)
* [ ] Cả khách vãng lai (không cần đơn)
* [x] Cả hai (có đơn + vãng lai)

---

### 27. Có phát thuốc gây nghiện, hướng thần không?

* [ ] Có
* [x] Không

---

### 28. Nhà thuốc đặt hàng từ nhà cung cấp nào? Có hệ thống đặt hàng sẵn có không?

Nhà thuốc đặt hàng từ nhiều nhà cung cấp.
Yêu cầu:

* Nhập kho nhà thuốc
* Cảnh báo hết hạn sử dụng
* Cảnh báo cơ số tối thiểu cần có

---

# E. Trung tâm kính

### 29. Nhà cung cấp tròng kính sử dụng?

* [x] Essilor
* [x] Hoya
* [ ] Zeiss
* [x] Khác: Việt Pháp

---

### 30. Hiện có hệ thống mã vạch cho gọng kính chưa?

* [ ] Đã có
* [x] Chưa, cần thiết lập từ đầu

---

### 31. Giá combo (gọng + tròng) được tính như thế nào?

* [ ] Combo định sẵn
* [ ] Nhân viên tạo combo tùy chỉnh
* [x] Cả hai

---

### 32. Cắt/mài tròng kính thực hiện ở đâu?

* [ ] Tại chỗ (in-house)
* [ ] Thuê ngoài
* [x] Cả hai

---

### 33. Kính áp tròng (mềm, ortho-k) bán qua kênh nào?

* [ ] Trung tâm kính
* [x] Kê đơn qua HIS
* [ ] Cả hai

---

# F. Bán hàng trực tuyến

### 34. Sản phẩm bán trực tuyến khi ra mắt?

* [ ] Tất cả sản phẩm quang học
* [ ] Chỉ thực phẩm chức năng/thuốc nhỏ mắt
* [x] Gọng kính/Kính mát
* [ ] Khác: Băng chườm ấm mắt (thiết bị y tế loại A)

Ghi chú:
Sau khai trương có thể triển khai giai đoạn 2 cho kính đơn, kính mát và toàn bộ phần liệt kê trên.
Hoặc ngày ra mắt không tập trung vào module bán hàng trực tuyến.

---

### 35. Cửa hàng trực tuyến phục vụ ai?

* [ ] Chỉ bệnh nhân phòng khám
* [x] Cả công chúng

---

### 36. Khách hàng trực tuyến có thể đặt lịch khám qua cùng hệ thống?

* [x] Có
* [ ] Không

---

### 37. Phương thức vận chuyển/giao hàng?

* [x] Tự giao
* [x] GHTK
* [x] GHN
* [x] Khác: Grab / Bee / Xanh SM

---

# G. Báo cáo & tài chính

### 38. Ai cần truy cập báo cáo tài chính?

* [x] Chủ phòng khám
* [x] Quản lý
* [x] Kế toán
* [ ] Khác: ___

---

### 39. Có hệ thống kế toán bên ngoài cần tích hợp không?

* [x] MISA
* [ ] Fast
* [ ] Không có
* [ ] Khác: ___

---

### 40. Các KPI quan trọng nhất trên dashboard?

* Doanh thu theo mảng (khám – kính – thuốc – liệu trình)
* Lãi gộp trung tâm kính
* Hiệu quả liệu trình Dry Eye (OSDI cải thiện)
* Doanh thu theo bác sĩ
* Số buổi liệu trình đã thực hiện / còn lại

---

### 41. Cần theo dõi hiệu suất theo bác sĩ?

* [x] Có (doanh thu, số bệnh nhân theo bác sĩ)
* [ ] Không

---