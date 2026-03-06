# Thanh toán & Tài chính (Billing & Finance) - User Stories

**Phạm vi:** Tạo hóa đơn tổng hợp từ tất cả các khoa (khám bệnh, dược phẩm, kính, điều trị), thu tiền bằng nhiều phương thức (tiền mặt, chuyển khoản, QR, thẻ), xuất hóa đơn điện tử theo luật Việt Nam, thanh toán gói điều trị 50/50, giảm giá với phê duyệt quản lý bằng mã PIN, hoàn tiền với phê duyệt quản lý/chủ phòng khám, nhật ký thay đổi giá, và quản lý ca làm việc với đối chiếu tiền mặt.
**Yêu cầu liên quan:** FIN-01, FIN-02, FIN-03, FIN-04, FIN-05, FIN-06, FIN-07, FIN-08, FIN-09, FIN-10, PRT-03
**Số lượng user stories:** 11

---

## US-FIN-001: Hóa đơn tổng hợp theo ca khám

**Là một** thu ngân, **Tôi muốn** xem hóa đơn tổng hợp của bệnh nhân gồm tất cả các dịch vụ (khám bệnh, dược phẩm, kính, điều trị), **Để** thu tiền chính xác một lần cho toàn bộ ca khám.

**Yêu cầu liên quan:** FIN-01, FIN-02

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Bệnh nhân hoàn tất các bước khám (khám bệnh, chẩn đoán, kê đơn, cấp phát thuốc, đặt kính, điều trị) → Hệ thống tự động tích lũy các mục phí vào hóa đơn theo từng dịch vụ được thực hiện
2. Thu ngân truy cập mục "Thanh toán" → "Hóa đơn" → Tìm bệnh nhân theo tên, số điện thoại, hoặc mã bệnh nhân → Hệ thống hiển thị hóa đơn tổng hợp với các dòng chi tiết (line items) nhóm theo khoa:
   - **Khám bệnh:** Phí khám, phí chẩn đoán, phí đo mắt
   - **Dược phẩm:** Các thuốc được cấp phát theo đơn
   - **Kính:** Gọng kính, tròng kính, phụ kiện
   - **Điều trị:** Gói điều trị IPL, LLLT, chăm sóc mi mắt
3. Mỗi nhóm khoa hiển thị tổng phụ (subtotal). Cuối hóa đơn hiển thị tổng cộng (grand total) bằng VNĐ
4. Hệ thống hiển thị thông tin bệnh nhân (họ tên, mã bệnh nhân, số điện thoại) và thông tin ca khám (ngày khám, bác sĩ phụ trách) ở phần đầu hóa đơn
5. Phân bổ doanh thu nội bộ (revenue allocation) theo từng khoa được lưu trữ riêng biệt cho mỗi dòng chi tiết, không hiển thị cho bệnh nhân

**Trường hợp ngoại lệ:**
1. Nếu bệnh nhân chỉ sử dụng một khoa (ví dụ chỉ khám bệnh, không mua thuốc) → Hệ thống vẫn hiển thị hóa đơn với một nhóm duy nhất
2. Nếu hóa đơn chưa có dịch vụ nào (bệnh nhân mới check-in) → Hệ thống hiển thị "Chưa có dịch vụ nào được ghi nhận" với tổng = 0 VNĐ
3. Nếu bệnh nhân mua thuốc OTC (không có ca khám) → Hệ thống tạo hóa đơn với VisitId = null, chỉ có nhóm "Dược phẩm"
4. Nếu dòng chi tiết bị hủy bỏ sau khi thêm → Hệ thống cập nhật tổng hóa đơn và đánh dấu dòng đã hủy

**Trường hợp lỗi:**
1. Khi không tải được hóa đơn → Hệ thống hiển thị toast lỗi: "Không thể tải hóa đơn. Vui lòng thử lại"
2. Khi không có quyền Billing.ViewInvoice → Hệ thống trả về lỗi 403 và chuyển hướng về trang chính
3. Khi dữ liệu ca khám bị thiếu (VisitId không hợp lệ) → Hệ thống hiển thị thông báo: "Không tìm thấy ca khám tương ứng"

### Ghi chú kỹ thuật
- Invoice aggregate root: InvoiceId, PatientId, VisitId (nullable cho OTC), InvoiceNumber, Status, CreatedAt, BranchId
- InvoiceLineItem entity: InvoiceId, Department (enum: Medical/Pharmacy/Optical/Treatment), Description, Quantity, UnitPrice, Amount, DepartmentRevenueAllocation
- Invoice được tạo tự động khi Visit bắt đầu, các line items thêm vào qua domain events (DrugDispensed, OpticalOrderCreated, TreatmentSessionRecorded, v.v.)
- API endpoints: GET /api/billing/invoices/{id}, GET /api/billing/invoices/by-visit/{visitId}
- Nhóm hiển thị bằng GROUP BY Department trên frontend

---

## US-FIN-002: Thanh toán bằng nhiều phương thức

**Là một** thu ngân, **Tôi muốn** thu tiền bằng nhiều phương thức (tiền mặt, chuyển khoản ngân hàng, QR, thẻ tín dụng/ghi nợ), **Để** phục vụ linh hoạt theo yêu cầu của bệnh nhân.

**Yêu cầu liên quan:** FIN-03

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Thu ngân mở hóa đơn của bệnh nhân → Hệ thống hiển thị tổng số tiền cần thanh toán
2. Thu ngân chọn phương thức thanh toán từ dropdown: Tiền mặt, Chuyển khoản ngân hàng, QR (VNPay/MoMo/ZaloPay), Thẻ (Visa/Mastercard)
3. Thu ngân nhập số tiền nhận → Nếu là tiền mặt, hệ thống tự động tính tiền thừa (số tiền nhận - tổng hóa đơn)
4. Thu ngân nhấn "Xác nhận thanh toán" → Hệ thống ghi nhận thanh toán, cập nhật trạng thái hóa đơn thành "Đã thanh toán" → Hiển thị "Thanh toán thành công"
5. Với phương thức QR: thu ngân chọn QR → Hệ thống hiển thị hướng dẫn "Bệnh nhân quét mã QR của phòng khám" → Thu ngân xác nhận đã nhận tiền thủ công
6. Với phương thức thẻ: thu ngân nhập loại thẻ (Visa/MC) và 4 số cuối → Hệ thống ghi nhận tham khảo

**Trường hợp ngoại lệ:**
1. Nếu số tiền thanh toán < tổng hóa đơn → Hệ thống cảnh báo: "Số tiền thanh toán chưa đủ. Còn thiếu [X] VNĐ"
2. Nếu bệnh nhân không có tiền mặt đủ → Thu ngân có thể chuyển sang phương thức khác hoặc tách thanh toán (xem US-FIN-003)
3. Nếu thu ngân nhập sai số tiền và muốn sửa → Thu ngân nhấn "Hủy" trước khi xác nhận → Quay lại form nhập
4. Nếu chuyển khoản chưa nhận được → Thu ngân ghi nhận "Chờ xác nhận" và cập nhật sau khi nhận tiền

**Trường hợp lỗi:**
1. Khi xác nhận thanh toán thất bại → Hệ thống hiển thị toast lỗi: "Thanh toán thất bại. Vui lòng thử lại"
2. Khi không có quyền Billing.ProcessPayment → Hệ thống trả về lỗi 403
3. Khi hóa đơn đã được thanh toán rồi → Hệ thống hiển thị: "Hóa đơn này đã được thanh toán"

### Ghi chú kỹ thuật
- Payment entity: PaymentId, InvoiceId, PaymentMethod (enum: Cash/BankTransfer/QR/Card), Amount, Reference (nullable — số giao dịch, 4 số cuối thẻ), Status (Completed/Pending), CreatedAt, CashierId, ShiftId
- Tất cả phương thức đều là manual confirmation (không có API tích hợp VNPay/MoMo cho v1)
- QR payment: phòng khám có static QR code, bệnh nhân quét và thu ngân xác nhận thủ công
- Cash calculation: ChangeAmount = ReceivedAmount - InvoiceTotal (chỉ áp dụng cho Cash)
- API endpoint: POST /api/billing/payments

---

## US-FIN-003: Tách thanh toán nhiều phương thức

**Là một** thu ngân, **Tôi muốn** tách thanh toán nhiều phương thức cho một hóa đơn (ví dụ: một phần tiền mặt, một phần QR), **Để** hỗ trợ bệnh nhân thanh toán linh hoạt khi không đủ tiền mặt hoặc muốn dùng nhiều phương thức.

**Yêu cầu liên quan:** FIN-03

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Thu ngân mở hóa đơn của bệnh nhân → Hệ thống hiển thị tổng cần thanh toán, ví dụ: 1.500.000 VNĐ
2. Thu ngân nhấn "Tách thanh toán" → Hệ thống hiển thị form cho phép thêm nhiều dòng thanh toán:
   - Dòng 1: Phương thức = Tiền mặt, Số tiền = 500.000 VNĐ
   - Dòng 2: Phương thức = QR (MoMo), Số tiền = 1.000.000 VNĐ
3. Hệ thống tự động tính tổng các dòng và so sánh với tổng hóa đơn. Hiển thị "Còn thiếu: [X] VNĐ" nếu chưa đủ hoặc "Đủ tiền" nếu đã khớp
4. Thu ngân nhấn "Xác nhận thanh toán" khi tổng các dòng = tổng hóa đơn → Hệ thống tạo nhiều bản ghi Payment cho một Invoice → Hiển thị "Thanh toán thành công"

**Trường hợp ngoại lệ:**
1. Nếu tổng các dòng thanh toán > tổng hóa đơn → Hệ thống cảnh báo: "Tổng thanh toán vượt quá số tiền cần thu [X] VNĐ"
2. Nếu thu ngân muốn xóa một dòng thanh toán → Nhấn nút xóa trên dòng đó → Hệ thống cập nhật lại tổng
3. Nếu chỉ có một dòng thanh toán → Hệ thống xử lý như thanh toán bình thường (US-FIN-002)
4. Thu ngân có thể thêm tối đa 4 dòng thanh toán cho một hóa đơn (giới hạn hợp lý)

**Trường hợp lỗi:**
1. Khi lưu thanh toán tách thất bại → Hệ thống hiển thị toast lỗi: "Thanh toán thất bại. Không có thay đổi nào được lưu"
2. Khi một trong các phương thức bị lỗi (ví dụ chuyển khoản thất bại) → Hệ thống cho phép thu ngân sửa hoặc xóa dòng đó và thử lại

### Ghi chú kỹ thuật
- Một Invoice có nhiều Payment records (1:N relationship)
- Invoice.PaidAmount = SUM(Payment.Amount WHERE Status = Completed)
- Invoice.Status chuyển thành Paid khi PaidAmount >= TotalAmount
- Validation: SUM(Payment amounts) phải = Invoice.TotalAmount trước khi xác nhận
- API endpoint: POST /api/billing/payments/split (nhận array PaymentRequest[])
- Transaction scope: tất cả payments trong một split được lưu trong một transaction

---

## US-FIN-004: Xuất hóa đơn điện tử

**Là một** kế toán, **Tôi muốn** xuất hóa đơn điện tử theo quy định pháp luật Việt Nam, **Để** nộp thuế và nhập vào phần mềm kế toán MISA.

**Yêu cầu liên quan:** FIN-04

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Kế toán truy cập "Thanh toán" → "Hóa đơn điện tử" → Hệ thống hiển thị danh sách hóa đơn đã thanh toán chưa xuất hóa đơn điện tử
2. Kế toán chọn một hoặc nhiều hóa đơn → Nhấn "Xuất hóa đơn điện tử" → Hệ thống tạo file hóa đơn điện tử bao gồm:
   - Thông tin người bán: Tên phòng khám, địa chỉ, mã số thuế, số điện thoại
   - Thông tin người mua: Tên bệnh nhân, địa chỉ (nếu có), mã số thuế (nếu có)
   - Chi tiết dịch vụ: STT, tên dịch vụ, đơn vị, số lượng, đơn giá, thành tiền
   - Tổng cộng, thuế GTGT (nếu áp dụng), tổng thanh toán
   - Số hóa đơn, ngày hóa đơn, ký hiệu mẫu
3. Hệ thống tạo cả hai định dạng:
   - **PDF**: Hóa đơn điện tử có thể in, theo mẫu chuẩn Việt Nam
   - **JSON/XML**: Dữ liệu có cấu trúc để nhập vào MISA thủ công
4. Kế toán tải file về máy → Nhập thủ công vào MISA (v1 không có API tự động)

**Trường hợp ngoại lệ:**
1. Nếu hóa đơn chưa được thanh toán → Hệ thống không cho phép xuất hóa đơn điện tử và hiển thị: "Chỉ có thể xuất hóa đơn điện tử cho hóa đơn đã thanh toán"
2. Nếu bệnh nhân không có thông tin mã số thuế → Hệ thống vẫn xuất hóa đơn với trường người mua để trống (hợp lệ theo quy định cho hóa đơn dưới 200.000 VNĐ)
3. Nếu hóa đơn đã được xuất hóa đơn điện tử trước đó → Hệ thống hiển thị cảnh báo: "Hóa đơn này đã được xuất hóa đơn điện tử ngày [DD/MM/YYYY]"
4. Nếu cần hủy/điều chỉnh hóa đơn điện tử → Kế toán tạo biên bản điều chỉnh theo quy định

**Trường hợp lỗi:**
1. Khi tạo PDF thất bại → Hệ thống hiển thị toast lỗi: "Tạo hóa đơn điện tử thất bại. Vui lòng thử lại"
2. Khi tạo JSON/XML thất bại → Hệ thống hiển thị chi tiết lỗi để kế toán báo cáo IT
3. Khi không có quyền Billing.ExportEInvoice → Hệ thống trả về lỗi 403

### Ghi chú kỹ thuật
- EInvoice entity: EInvoiceId, InvoiceId, EInvoiceNumber, IssuedAt, PdfUrl, JsonXmlUrl, Status
- BillingDocumentService sử dụng QuestPDF (đã tích hợp Phase 5) để tạo PDF hóa đơn điện tử
- EInvoiceExportService tạo JSON và XML theo cấu trúc tương thích MISA
- Clinic header lấy từ ConfigurableClinicSettings (Phase 5)
- API endpoints: POST /api/billing/e-invoices/generate, GET /api/billing/e-invoices/{id}/pdf, GET /api/billing/e-invoices/{id}/export

---

## US-FIN-005: Thanh toán gói điều trị (50/50 split)

**Là một** thu ngân, **Tôi muốn** ghi nhận thanh toán gói điều trị với phương thức thanh toán full hoặc chia 50/50 (IsSplitPayment và SplitSequence), **Để** hệ thống lưu trữ dữ liệu thanh toán chia đôi cho gói điều trị.

**Yêu cầu liên quan:** FIN-05, FIN-06

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Thu ngân mở hóa đơn có gói điều trị (IPL, LLLT, chăm sóc mi mắt) → Hệ thống hiển thị thông tin gói: tên gói, số buổi, tổng giá
2. Thu ngân chọn phương thức thanh toán gói:
   - **Thanh toán full:** Bệnh nhân trả toàn bộ giá gói một lần → Hệ thống ghi nhận IsSplitPayment = false
   - **Chia 50/50:** Bệnh nhân trả 50% lần đầu → Hệ thống ghi nhận IsSplitPayment = true, SplitSequence = 1, TreatmentPackageId liên kết
3. Với chia 50/50, lần đầu (SplitSequence = 1): thu ngân thu 50% giá gói → Hệ thống ghi nhận và tạo Payment với số tiền = 50% tổng giá
4. Với chia 50/50, lần hai (SplitSequence = 2): trước buổi điều trị giữa khóa (buổi 3/5 hoặc buổi 2/3), thu ngân thu 50% còn lại → Hệ thống ghi nhận SplitSequence = 2 và cập nhật trạng thái thanh toán gói thành "Đã thanh toán đủ"

**Quan trọng — Phạm vi Phase 7 và Phase 9:**
- **Phase 7 (hiện tại):** Hệ thống ghi nhận dữ liệu thanh toán 50/50 (IsSplitPayment flag, SplitSequence tracking, TreatmentPackageId link). Thu ngân có thể xem trạng thái thanh toán gói điều trị.
- **Phase 9 (Treatment):** Kiểm soát buổi điều trị giữa khóa sẽ được thực hiện ở Phase 9. Phase 9 sẽ kiểm tra "bệnh nhân đã thanh toán lần 2 chưa?" trước khi cho phép bắt đầu buổi điều trị giữa khóa. Phase 7 chỉ lưu dữ liệu, không chặn buổi điều trị.

**Trường hợp ngoại lệ:**
1. Nếu bệnh nhân đổi từ thanh toán full sang 50/50 sau khi đã bắt đầu → Hệ thống không cho phép thay đổi (phải hủy và tạo lại gói)
2. Nếu bệnh nhân chưa thanh toán lần 2 → Hệ thống hiển thị cảnh báo "Chưa thanh toán lần 2" trong danh sách gói điều trị (chỉ cảnh báo, không chặn — việc chặn do Phase 9 xử lý)
3. Nếu gói điều trị bị hủy giữa chừng → Thu ngân tạo yêu cầu hoàn tiền (xem US-FIN-007)
4. Nếu số tiền thanh toán không chính xác 50% (ví dụ bệnh nhân muốn trả 60/40) → Hệ thống chỉ hỗ trợ 50/50, không cho tỷ lệ khác

**Trường hợp lỗi:**
1. Khi ghi nhận thanh toán gói thất bại → Hệ thống hiển thị toast lỗi: "Ghi nhận thanh toán gói thất bại"
2. Khi không tìm thấy gói điều trị liên kết → Hệ thống hiển thị: "Không tìm thấy gói điều trị tương ứng"
3. Khi có xung đột SplitSequence (đã có SplitSequence = 2 rồi) → Hệ thống từ chối và hiển thị: "Gói điều trị đã được thanh toán đầy đủ"

### Ghi chú kỹ thuật
- Payment entity mở rộng: IsSplitPayment (bool), SplitSequence (int? — 1 hoặc 2), TreatmentPackageId (Guid? — liên kết đến gói điều trị)
- Tính 50%: SplitAmount = TreatmentPackage.TotalPrice / 2 (làm tròn lên đến hàng nghìn VNĐ)
- Quy tắc giữa khóa (Phase 9): gói 5 buổi → lần 2 trước buổi 3; gói 3 buổi → lần 2 trước buổi 2
- Phase 7 chỉ lưu dữ liệu, không enforce blocking logic
- API endpoints: POST /api/billing/payments/treatment-package, GET /api/billing/treatment-packages/{id}/payment-status

---

## US-FIN-006: Giảm giá với phê duyệt quản lý

**Là một** thu ngân, **Tôi muốn** áp dụng giảm giá (phần trăm hoặc số tiền cố định) với sự phê duyệt của quản lý bằng mã PIN, **Để** đảm bảo mọi giảm giá đều được kiểm soát và có người chịu trách nhiệm.

**Yêu cầu liên quan:** FIN-07

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Thu ngân mở hóa đơn của bệnh nhân → Nhấn "Áp dụng giảm giá" → Hệ thống hiển thị form giảm giá:
   - Loại giảm giá: Phần trăm (%) hoặc Số tiền cố định (VNĐ)
   - Giá trị: nhập phần trăm (ví dụ: 10%) hoặc số tiền (ví dụ: 200.000 VNĐ)
   - Phạm vi: Toàn hóa đơn hoặc Dòng chi tiết cụ thể
   - Lý do giảm giá (bắt buộc): text field
2. Hệ thống tính số tiền giảm và hiển thị số tiền sau giảm để thu ngân kiểm tra trước khi gửi yêu cầu phê duyệt
3. Thu ngân nhấn "Yêu cầu phê duyệt" → Hệ thống hiển thị dialog yêu cầu mã PIN quản lý → Quản lý nhập mã PIN
4. Hệ thống xác minh mã PIN là đúng và thuộc user có quyền Manager → Áp dụng giảm giá vào hóa đơn → Hiển thị "Giảm giá đã được phê duyệt và áp dụng"
5. Hóa đơn cập nhật: hiển thị dòng "Giảm giá" với số tiền âm, tổng hóa đơn được tính lại

**Trường hợp ngoại lệ:**
1. Nếu giảm giá phần trăm > 50% → Hệ thống yêu cầu phê duyệt của chủ phòng khám (OwnerPin) thay vì quản lý
2. Nếu giảm giá số tiền > tổng hóa đơn → Hệ thống từ chối: "Số tiền giảm giá không được vượt quá tổng hóa đơn"
3. Nếu hóa đơn đã có giảm giá → Hệ thống hiển thị giảm giá hiện tại và cho phép thay đổi (cần phê duyệt lại)
4. Nếu quản lý không có mặt → Thu ngân thông báo cho bệnh nhân chờ đợi hoặc thanh toán bình thường

**Trường hợp lỗi:**
1. Khi mã PIN sai → Hệ thống hiển thị: "Mã PIN không đúng. Vui lòng thử lại" (tối đa 3 lần)
2. Khi mã PIN đúng nhưng user không có quyền Manager → Hệ thống hiển thị: "Tài khoản không có quyền phê duyệt giảm giá"
3. Khi áp dụng giảm giá thất bại → Hệ thống hiển thị toast lỗi và không thay đổi hóa đơn
4. Sau 3 lần nhập PIN sai → Hệ thống khóa chức năng giảm giá cho hóa đơn này trong 15 phút

### Ghi chú kỹ thuật
- Discount entity: DiscountId, InvoiceId, InvoiceLineItemId (nullable — null = toàn hóa đơn), DiscountType (Percentage/FixedAmount), Value, CalculatedAmount, Reason, ApprovedByUserId, ApprovedAt, Status (Pending/Approved/Rejected)
- Manager PIN: lưu trong UserProfile.ApprovalPin (hashed BCrypt), kiểm tra quyền Billing.ApproveDiscount
- PIN verification: POST /api/billing/discounts/verify-pin → trả về ApprovalToken (JWT ngắn hạn 5 phút)
- Apply discount: POST /api/billing/discounts/apply (cần ApprovalToken trong header)
- Audit trail: DiscountApproval event ghi vào AuditLog

---

## US-FIN-007: Hoàn tiền với phê duyệt

**Là một** thu ngân, **Tôi muốn** yêu cầu hoàn tiền với sự phê duyệt của quản lý hoặc chủ phòng khám, **Để** xử lý chính xác và có nhật ký đầy đủ cho mọi giao dịch hoàn tiền.

**Yêu cầu liên quan:** FIN-08

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Thu ngân truy cập "Thanh toán" → "Hoàn tiền" → Tìm hóa đơn đã thanh toán bằng số hóa đơn hoặc tên bệnh nhân
2. Thu ngân chọn hóa đơn cần hoàn → Hệ thống hiển thị chi tiết hóa đơn và các dòng chi tiết
3. Thu ngân chọn loại hoàn tiền:
   - **Hoàn toàn phần:** Hoàn toàn bộ số tiền hóa đơn
   - **Hoàn một phần:** Chọn dòng chi tiết cụ thể và nhập số tiền hoàn cho từng dòng
4. Thu ngân nhập lý do hoàn tiền (bắt buộc) và nhấn "Gửi yêu cầu hoàn tiền"
5. Hệ thống tạo yêu cầu hoàn tiền với trạng thái "Chờ phê duyệt" → Thông báo đến quản lý/chủ phòng khám
6. Quản lý/chủ phòng khám xem yêu cầu hoàn tiền → Xem chi tiết và lý do → Nhấn "Phê duyệt" (với mã PIN) hoặc "Từ chối" (với lý do từ chối)
7. Nếu được phê duyệt → Hệ thống tạo bản ghi hoàn tiền, cập nhật trạng thái hóa đơn, và ghi nhật ký đầy đủ

**Trường hợp ngoại lệ:**
1. Nếu hóa đơn đã quá 30 ngày → Hệ thống cảnh báo: "Hóa đơn đã quá 30 ngày. Hoàn tiền cần phê duyệt của chủ phòng khám (không chỉ quản lý)"
2. Nếu hóa đơn đã được xuất hóa đơn điện tử → Hệ thống yêu cầu tạo biên bản điều chỉnh trước khi hoàn tiền
3. Nếu số tiền hoàn một phần > số tiền dòng chi tiết → Hệ thống từ chối: "Số tiền hoàn không được vượt quá giá trị dòng chi tiết"
4. Nếu hóa đơn đã được hoàn tiền trước đó → Hệ thống hiển thị lịch sử hoàn tiền và giới hạn số tiền hoàn còn lại

**Trường hợp lỗi:**
1. Khi tạo yêu cầu hoàn tiền thất bại → Hệ thống hiển thị toast lỗi: "Tạo yêu cầu hoàn tiền thất bại"
2. Khi phê duyệt hoàn tiền thất bại → Hệ thống hiển thị toast lỗi và giữ trạng thái yêu cầu
3. Khi không có quyền Billing.RequestRefund (thu ngân) hoặc Billing.ApproveRefund (quản lý) → Hệ thống trả về lỗi 403

### Ghi chú kỹ thuật
- Refund entity: RefundId, InvoiceId, RefundType (Full/Partial), TotalRefundAmount, Reason, RequestedByUserId, RequestedAt, ApprovedByUserId, ApprovedAt, Status (Pending/Approved/Rejected/Processed), RejectionReason
- RefundLineItem: RefundId, InvoiceLineItemId, RefundAmount
- Refund workflow: Pending → Approved → Processed (hoặc Pending → Rejected)
- Processing: tạo Payment với Amount âm (refund payment) liên kết với ShiftId hiện tại
- API endpoints: POST /api/billing/refunds/request, POST /api/billing/refunds/{id}/approve, POST /api/billing/refunds/{id}/reject, POST /api/billing/refunds/{id}/process

---

## US-FIN-008: Nhật ký thay đổi giá

**Là một** quản lý, **Tôi muốn** hệ thống tự động ghi nhật ký mọi thay đổi giá (ai thay đổi, khi nào, giá cũ và giá mới), **Để** kiểm tra và truy vết chính xác lịch sử thay đổi giá dịch vụ.

**Yêu cầu liên quan:** FIN-09

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Quản lý thay đổi giá một dịch vụ (ví dụ: tăng phí khám từ 200.000 VNĐ lên 250.000 VNĐ) → Hệ thống tự động ghi vào nhật ký thay đổi giá bao gồm:
   - Tên dịch vụ / sản phẩm bị thay đổi giá
   - Giá cũ (OldPrice)
   - Giá mới (NewPrice)
   - Người thay đổi (UserId, UserName)
   - Thời gian thay đổi (timestamp)
   - Lý do thay đổi (nếu có)
2. Quản lý truy cập "Thanh toán" → "Nhật ký giá" → Hệ thống hiển thị bảng lịch sử thay đổi giá với các cột: Ngày, Dịch vụ, Giá cũ, Giá mới, Thay đổi (%), Người thay đổi
3. Quản lý có thể lọc theo khoảng thời gian, loại dịch vụ (khám bệnh / thuốc / kính / điều trị), hoặc người thay đổi
4. Hệ thống hiển thị chart xu hướng giá cho từng dịch vụ khi quản lý nhấn vào dòng chi tiết

**Trường hợp ngoại lệ:**
1. Nếu giá không thay đổi (giá cũ = giá mới) → Hệ thống không tạo bản ghi nhật ký
2. Nếu nhiều dịch vụ thay đổi giá cùng lúc (cập nhật hàng loạt) → Hệ thống ghi từng dòng riêng biệt cho mỗi dịch vụ
3. Nếu giá bị thay đổi về lại giá cũ (hoàn nguyên) → Hệ thống vẫn ghi một bản ghi mới, không xóa bản ghi trước đó

**Trường hợp lỗi:**
1. Khi không tải được nhật ký → Hệ thống hiển thị toast lỗi: "Không thể tải nhật ký thay đổi giá"
2. Khi không có quyền Billing.ViewPriceAudit → Hệ thống trả về lỗi 403

### Ghi chú kỹ thuật
- PriceChangeLog entity: Id, EntityType (Service/Drug/Frame/Lens/Treatment), EntityId, EntityName, OldPrice, NewPrice, ChangedByUserId, ChangedByUserName, ChangedAt, Reason
- Sử dụng Audit interceptor (đã có) để tự động capture thay đổi giá trên các entity có trường Price/UnitPrice/SellingPrice
- PriceChangeLog là bảng immutable (chỉ INSERT, không UPDATE/DELETE) — lưu trữ vĩnh viễn
- API endpoints: GET /api/billing/price-audit-log?from={date}&to={date}&type={entityType}
- Không cần UI riêng để nhập thay đổi giá — nhật ký được ghi tự động khi giá thay đổi từ bất kỳ màn hình nào

---

## US-FIN-009: Quản lý ca làm việc

**Là một** thu ngân, **Tôi muốn** mở ca làm việc, thu tiền trong ca, và đóng ca với đối chiếu tiền mặt, **Để** quản lý doanh thu chính xác theo từng ca làm việc.

**Yêu cầu liên quan:** FIN-10

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Thu ngân đăng nhập vào hệ thống → Hệ thống kiểm tra có ca làm việc đang mở không:
   - Nếu chưa có ca → Hiển thị dialog "Mở ca làm việc" với: Mẫu ca (Sáng / Chiều, lấy từ template có sẵn), Thời gian bắt đầu (mặc định = giờ hiện tại, có thể chỉnh), Số dư tiền mặt đầu ca (nhập tay)
   - Nếu đã có ca đang mở → Tiếp tục làm việc bình thường
2. Thu ngân nhập số dư tiền mặt đầu ca và nhấn "Mở ca" → Hệ thống tạo Shift mới với trạng thái "Đang hoạt động"
3. Trong suốt ca làm việc, mọi giao dịch thanh toán và hoàn tiền đều được liên kết với ShiftId hiện tại
4. Khi hết ca, thu ngân nhấn "Đóng ca" → Hệ thống hiển thị form đối chiếu:
   - Tiền mặt kỳ vọng (OpeningBalance + CashReceived - CashRefunds)
   - Thu ngân nhập số tiền mặt thực tế đếm được
   - Hệ thống tính chênh lệch (Thực tế - Kỳ vọng)
   - Thu ngân nhập ghi chú (bắt buộc nếu chênh lệch != 0)
5. Thu ngân nhấn "Xác nhận đóng ca" → Hệ thống đóng Shift và tạo báo cáo ca

**Trường hợp ngoại lệ:**
1. Nếu thu ngân quên đóng ca cuối ngày trước → Hệ thống cho phép đóng ca trước rồi mở ca mới. Cảnh báo: "Ca trước chưa được đóng. Vui lòng đóng ca [X] trước"
2. Nếu có chênh lệch tiền mặt lớn (> 500.000 VNĐ) → Hệ thống yêu cầu phê duyệt quản lý trước khi đóng ca
3. Nếu thu ngân khác muốn mở ca khi đã có ca đang mở của người khác (cùng chi nhánh) → Hệ thống cho phép (hỗ trợ đồng thời nhiều thu ngân)
4. Nếu không có giao dịch nào trong ca → Hệ thống vẫn cho phép đóng ca với báo cáo trống

**Trường hợp lỗi:**
1. Khi mở ca thất bại → Hệ thống hiển thị toast lỗi: "Mở ca làm việc thất bại"
2. Khi đóng ca thất bại → Hệ thống hiển thị toast lỗi và giữ ca ở trạng thái "Đang hoạt động"
3. Khi không có quyền Billing.ManageShift → Hệ thống trả về lỗi 403

### Ghi chú kỹ thuật
- Shift entity: ShiftId, CashierId, BranchId, ShiftTemplateId, StartTime, EndTime, OpeningBalance, ExpectedCash, ActualCash, Discrepancy, DiscrepancyNote, Status (Open/Closed), ManagerApprovalId (nullable)
- ShiftTemplate entity: Id, Name (Sáng/Chiều), DefaultStartTime, DefaultEndTime, BranchId
- Shift templates mặc định: Sáng (08:00-12:00 cho Thứ 7, Chủ Nhật), Chiều (13:00-20:00 cho Thứ 3 - Thứ 6)
- Mọi Payment/Refund có ShiftId để liên kết với ca làm việc
- ExpectedCash = OpeningBalance + SUM(Cash Payments) - SUM(Cash Refunds)
- API endpoints: POST /api/billing/shifts/open, POST /api/billing/shifts/{id}/close, GET /api/billing/shifts/current

---

## US-FIN-010: Báo cáo ca làm việc

**Là một** quản lý, **Tôi muốn** xem báo cáo ca làm việc gồm doanh thu theo phương thức thanh toán và chênh lệch tiền mặt, **Để** giám sát tài chính hàng ngày và phát hiện bất thường kịp thời.

**Yêu cầu liên quan:** FIN-10

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Quản lý truy cập "Thanh toán" → "Báo cáo ca" → Hệ thống hiển thị danh sách ca làm việc đã đóng với: Ngày, Thu ngân, Giờ bắt đầu/kết thúc, Tổng doanh thu, Chênh lệch tiền mặt, Trạng thái
2. Quản lý chọn một ca → Hệ thống hiển thị báo cáo chi tiết:
   - **Tổng quan:** Tổng doanh thu, số giao dịch, số hóa đơn
   - **Doanh thu theo phương thức:** Tiền mặt | Chuyển khoản | QR | Thẻ — số tiền và số giao dịch mỗi loại
   - **Hoàn tiền:** Tổng số tiền hoàn, số giao dịch hoàn
   - **Đối chiếu tiền mặt:** Số dư đầu ca, Tiền mặt nhận, Tiền mặt hoàn, Tiền mặt kỳ vọng, Tiền mặt thực tế, Chênh lệch, Ghi chú
3. Quản lý nhấn "In báo cáo" → Hệ thống tạo PDF báo cáo ca với QuestPDF (có header phòng khám) → Tải về hoặc in trực tiếp
4. Quản lý có thể lọc danh sách ca theo khoảng thời gian và thu ngân

**Trường hợp ngoại lệ:**
1. Nếu ca chưa đóng → Hệ thống hiển thị trạng thái "Đang hoạt động" và không cho xem báo cáo chi tiết (chỉ hiển thị số liệu tạm thời)
2. Nếu ca không có giao dịch → Báo cáo hiển thị tất cả giá trị = 0 với ghi chú "Không có giao dịch trong ca"
3. Nếu chênh lệch tiền mặt > 0 → Hiển thị màu đỏ và biểu tượng cảnh báo để quản lý dễ dàng nhận biết
4. Nếu nhiều ca trong cùng ngày → Hệ thống hiển thị từng ca riêng biệt và có tổng hợp theo ngày

**Trường hợp lỗi:**
1. Khi tạo PDF báo cáo thất bại → Hệ thống hiển thị toast lỗi: "Tạo báo cáo thất bại. Vui lòng thử lại"
2. Khi không có quyền Billing.ViewShiftReport → Hệ thống trả về lỗi 403
3. Khi dữ liệu ca bị thiếu (shift bị corrupted) → Hệ thống hiển thị cảnh báo và số liệu có sẵn

### Ghi chú kỹ thuật
- ShiftReport được tính từ các Payment/Refund records liên kết với ShiftId
- Revenue by method: GROUP BY PaymentMethod WHERE ShiftId = {shiftId}
- PDF report sử dụng QuestPDF với ClinicHeaderTemplate (Phase 5)
- API endpoints: GET /api/billing/shifts/{id}/report, GET /api/billing/shifts/{id}/report/pdf
- Danh sách ca: GET /api/billing/shifts?from={date}&to={date}&cashierId={id}

---

## US-FIN-011: In hóa đơn và phiếu thu

**Là một** thu ngân, **Tôi muốn** in hóa đơn và phiếu thu cho bệnh nhân sau khi thanh toán, **Để** cung cấp chứng từ thanh toán cho bệnh nhân.

**Yêu cầu liên quan:** PRT-03

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Sau khi thanh toán thành công → Hệ thống tự động hiển thị preview hóa đơn/phiếu thu với nút "In" và "Tải về PDF"
2. Thu ngân nhấn "In" → Hệ thống tạo PDF hóa đơn với:
   - **Header:** Logo phòng khám, tên phòng khám, địa chỉ, số điện thoại, mã số thuế, số giấy phép
   - **Thông tin bệnh nhân:** Họ tên, mã bệnh nhân, số điện thoại
   - **Chi tiết dịch vụ:** Bảng danh sách dòng chi tiết nhóm theo khoa với STT, tên dịch vụ, đơn vị, số lượng, đơn giá, thành tiền
   - **Giảm giá (nếu có):** Hiển thị dòng giảm giá với số tiền âm
   - **Tổng cộng:** Tổng thanh toán bằng số và bằng chữ (VNĐ)
   - **Phương thức thanh toán:** Ghi rõ các phương thức đã sử dụng (ví dụ: "Tiền mặt: 500.000 VNĐ, QR MoMo: 1.000.000 VNĐ")
   - **Footer:** Số hóa đơn, ngày xuất, thu ngân ký tên
3. Thu ngân nhấn "In phiếu thu" → Hệ thống tạo phiếu thu (receipt) nhỏ gọn hơn, thích hợp để in trên máy in nhiệt (thermal printer) kích thước 80mm
4. Bệnh nhân nhận hóa đơn/phiếu thu giấy

**Trường hợp ngoại lệ:**
1. Nếu hóa đơn có giảm giá → Hiển thị dòng giảm giá riêng với ghi chú lý do (ví dụ: "Giảm giá 10% — khuyến mãi")
2. Nếu hóa đơn có nhiều phương thức thanh toán → Liệt kê tất cả phương thức trên phiếu thu
3. Nếu bệnh nhân yêu cầu in lại → Thu ngân tìm hóa đơn và nhấn "In lại" → Hệ thống in với ghi chú "Bản sao" (COPY)
4. Nếu máy in không kết nối → Hệ thống hiển thị bản xem trước để thu ngân tải về PDF và in thủ công

**Trường hợp lỗi:**
1. Khi tạo PDF thất bại → Hệ thống hiển thị toast lỗi: "Tạo hóa đơn thất bại. Vui lòng thử lại"
2. Khi kết nối máy in thất bại → Hệ thống hiển thị tùy chọn tải về PDF thay vì in trực tiếp
3. Khi không có quyền Billing.PrintInvoice → Hệ thống trả về lỗi 403

### Ghi chú kỹ thuật
- BillingDocumentService sử dụng QuestPDF để tạo PDF hóa đơn (A4) và phiếu thu (80mm thermal)
- Clinic header lấy từ ConfigurableClinicSettings (tên, địa chỉ, logo, MST, giấy phép)
- Số tiền bằng chữ: sử dụng thư viện chuyển đổi số thành tiếng Việt (ví dụ: "Một triệu năm trăm nghìn đồng")
- Print endpoints: GET /api/billing/invoices/{id}/print (A4 PDF), GET /api/billing/invoices/{id}/receipt (thermal receipt PDF)
- Bản sao (COPY): thêm watermark "BẢN SAO" khi in lại

---

*Phiên bản: 1.0*
*Ngày tạo: 2026-03-06*
*Giai đoạn: Phase 7 - Billing & Finance (Thanh toán & Tài chính)*
*Yêu cầu được bao phủ: FIN-01, FIN-02, FIN-03, FIN-04, FIN-05, FIN-06, FIN-07, FIN-08, FIN-09, FIN-10, PRT-03*
