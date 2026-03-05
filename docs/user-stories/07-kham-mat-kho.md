# Khám Mắt Khô (Dry Eye Assessment) - User Stories

**Phạm vi:** Khám và đánh giá mắt khô bao gồm: ghi nhận dữ liệu đánh giá mắt khô có cấu trúc (OSDI, TBUT, Schirmer, phân độ tuyến Meibomius, bờ mi nước mắt, điểm nhuộm), tính toán và phân loại mức độ OSDI tự động với mã màu, biểu đồ xu hướng OSDI qua các lần khám, so sánh chỉ số mắt khô giữa các lần khám, và bệnh nhân tự điền OSDI qua QR code.
**Yêu cầu liên quan:** DRY-01, DRY-02, DRY-03, DRY-04
**Số lượng user stories:** 8

---

## US-DRY-001: Bác sĩ ghi nhận dữ liệu đánh giá mắt khô có cấu trúc

**Là một** bác sĩ, **Tôi muốn** ghi nhận dữ liệu đánh giá mắt khô có cấu trúc cho từng mắt bao gồm TBUT, Schirmer, phân độ tuyến Meibomius, bờ mi nước mắt và điểm nhuộm, **Để** lưu trữ đầy đủ kết quả khám phục vụ chẩn đoán và theo dõi tiến triển bệnh mắt khô.

**Yêu cầu liên quan:** DRY-01

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Bác sĩ mở lượt khám và chọn phần "Đánh giá mắt khô" → Hệ thống hiển thị DryEyeSection dạng VisitSection (thu gọn được) với bố cục lưới OD/OS song song (tương tự RefractionForm)
2. Bác sĩ nhập TBUT (giây) cho mỗi mắt → Hệ thống tự động lưu khi rời trường (auto-save on blur, debounce 500ms)
3. Bác sĩ nhập Schirmer (mm) cho mỗi mắt → Hệ thống lưu tự động
4. Bác sĩ chọn phân độ tuyến Meibomius theo thang Arita 0-3 (0=không mất, 1=<33%, 2=33-66%, 3=>66%) cho mỗi mắt → Hệ thống lưu tự động
5. Bác sĩ nhập bờ mi nước mắt (Tear meniscus, mm) cho mỗi mắt → Hệ thống lưu tự động
6. Bác sĩ nhập điểm nhuộm (Staining score) cho mỗi mắt → Hệ thống lưu tự động
7. Hệ thống hiển thị thông báo "Đã lưu đánh giá mắt khô" sau mỗi lần lưu thành công

**Trường hợp ngoại lệ:**
1. Nếu bác sĩ chỉ khám một mắt → Cho phép để trống các trường của mắt còn lại
2. Nếu không cần ghi nhận một số chỉ số → Cho phép để trống (trường không bắt buộc)
3. Nếu bệnh nhân đã có dữ liệu mắt khô từ lần khám trước → Hệ thống hiển thị giá trị lần khám trước để tham khảo

**Trường hợp lỗi:**
1. Khi lưu thất bại → Hệ thống hiển thị toast lỗi: "Lưu dữ liệu đánh giá mắt khô thất bại"
2. Khi nhập giá trị ngoài phạm vi hợp lệ (ví dụ: TBUT âm, Schirmer âm) → Hệ thống validation từ chối và hiển thị cảnh báo
3. Khi token hết hạn trong lúc lưu → Hệ thống tự động refresh token và thử lại, nếu thất bại hiển thị yêu cầu đăng nhập lại

### Ghi chú kỹ thuật
- DryEyeSection sử dụng VisitSection wrapper với headerExtra slot
- Bố cục lưới grid-cols-[80px_1fr_1fr] giống RefractionForm cho OD/OS song song
- Auto-save on blur (debounced 500ms) nhất quán với Refraction pattern
- Các trường decimal sử dụng precision(5,2)
- Phân độ tuyến Meibomius: thang Arita 0-3 (standard trong nhãn khoa Việt Nam)
- Hồ sơ chỉ đọc (read-only) khi lượt khám đã ký duyệt
- API endpoint: POST/PUT /api/clinical/visits/{visitId}/dry-eye

---

## US-DRY-002: Bác sĩ ghi nhận điểm OSDI trong lượt khám

**Là một** bác sĩ, **Tôi muốn** ghi nhận bộ câu hỏi OSDI 12 câu với đầy đủ nội dung tiếng Việt trong phần đánh giá mắt khô, **Để** có dữ liệu triệu chứng chủ quan của bệnh nhân phục vụ đánh giá mức độ và theo dõi điều trị.

**Yêu cầu liên quan:** DRY-01

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Bác sĩ mở phần "Đánh giá mắt khô" → Hệ thống hiển thị 12 câu hỏi OSDI đầy đủ nội dung tiếng Việt (không viết tắt)
2. Bác sĩ ghi nhận câu trả lời của bệnh nhân cho từng câu (thang điểm 0-4: Luôn luôn / Hầu hết thời gian / Một nửa thời gian / Thỉnh thoảng / Không bao giờ) → Hệ thống cập nhật điểm OSDI tự động theo công thức
3. Hệ thống tính OSDI = (Tổng điểm x 25) / Số câu trả lời → Hiển thị kết quả theo thang 0-100
4. Bác sĩ hoàn thành bộ câu hỏi → Hệ thống lưu tự động kết quả OSDI

**Trường hợp ngoại lệ:**
1. Nếu bệnh nhân không trả lời được một số câu → Hệ thống tính OSDI dựa trên số câu đã trả lời (mẫu số thay đổi)
2. Nếu bệnh nhân không trả lời câu nào → Hệ thống không tính điểm OSDI, hiển thị "Chưa có dữ liệu OSDI"
3. Nếu bệnh nhân đã tự điền OSDI qua QR code (Xem thêm: US-DRY-007) → Hệ thống hiển thị kết quả đã điền, bác sĩ có thể xem và chỉnh sửa

**Trường hợp lỗi:**
1. Khi lưu OSDI thất bại → Hệ thống hiển thị toast lỗi: "Lưu điểm OSDI thất bại"
2. Khi điểm OSDI tính ra ngoài phạm vi 0-100 → Hệ thống validation từ chối

### Ghi chú kỹ thuật
- OSDI là điểm triệu chứng chủ quan của bệnh nhân (không phải per-eye) - 12 câu hỏi về cảm giác khó chịu tổng thể
- Công thức OSDI: (Tổng điểm x 25) / Số câu trả lời hợp lệ
- 12 câu OSDI chia 3 nhóm: Tầm nhìn (câu 1-5), Hoạt động (câu 6-9), Yếu tố môi trường (câu 10-12)
- Mỗi câu có 5 mức: 0 (Không bao giờ), 1 (Thỉnh thoảng), 2 (Một nửa thời gian), 3 (Hầu hết thời gian), 4 (Luôn luôn)
- Tất cả React Query mutations phải có onError callback với toast.error

---

## US-DRY-003: Hệ thống tính toán và hiển thị phân loại mức độ OSDI

**Là một** bác sĩ, **Tôi muốn** hệ thống tự động tính toán điểm OSDI và hiển thị phân loại mức độ với mã màu trực quan, **Để** nhanh chóng đánh giá tình trạng mắt khô của bệnh nhân.

**Yêu cầu liên quan:** DRY-02

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Bác sĩ hoàn thành bộ câu hỏi OSDI → Hệ thống tự động tính điểm OSDI theo công thức: (Tổng điểm x 25) / Số câu trả lời
2. Hệ thống hiển thị mức độ với mã màu:
   - **Bình thường** (0-12): badge màu xanh lá (green)
   - **Nhẹ** (13-22): badge màu vàng (yellow)
   - **Trung bình** (23-32): badge màu cam (orange)
   - **Nặng** (33-100): badge màu đỏ (red)
3. Badge mức độ hiển thị trong headerExtra của DryEyeSection → Bác sĩ nhìn thấy ngay khi mở lượt khám
4. Điểm OSDI và mức độ cập nhật realtime khi bác sĩ thay đổi câu trả lời

**Trường hợp ngoại lệ:**
1. Nếu chưa trả lời đủ câu hỏi → Hệ thống vẫn tính và hiển thị dựa trên số câu đã trả lời
2. Nếu chưa có dữ liệu OSDI → Không hiển thị badge mức độ
3. Nếu bệnh nhân có lượt khám trước → Hiển thị so sánh xu hướng (tăng/giảm) bên cạnh badge mức độ hiện tại

**Trường hợp lỗi:**
1. Khi tính toán OSDI cho kết quả không hợp lệ → Hệ thống hiển thị "Không xác định" thay vì badge lỗi

### Ghi chú kỹ thuật
- Mã màu severity sử dụng CSS variables tương thích với shadcn/ui theme
- Badge hiển thị trong VisitSection headerExtra slot
- Phân loại: Normal (0-12, green), Mild (13-22, yellow), Moderate (23-32, orange), Severe (33-100, red)
- Tính toán OSDI thực hiện phía client (realtime feedback) và backend (persistence + validation)

---

## US-DRY-004: Hiển thị chi tiết phân loại mức độ OSDI cho bệnh nhân

**Là một** bác sĩ, **Tôi muốn** xem chi tiết phân loại mức độ OSDI bao gồm điểm số, nhóm triệu chứng và giải thích lâm sàng, **Để** giải thích kết quả cho bệnh nhân và đưa ra quyết định điều trị phù hợp.

**Yêu cầu liên quan:** DRY-02

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Bác sĩ nhấn vào badge mức độ OSDI → Hệ thống hiển thị chi tiết phân loại:
   - Điểm OSDI tổng: X/100
   - Phân loại mức độ với mã màu
   - Điểm theo nhóm: Tầm nhìn (câu 1-5), Hoạt động (câu 6-9), Môi trường (câu 10-12)
2. Hệ thống hiển thị thanh tiến trình (progress bar) thể hiện vị trí điểm OSDI trên thang 0-100 với các vùng màu tương ứng
3. Nếu có dữ liệu lần khám trước → Hiển thị so sánh: "Lần khám trước: X điểm → Lần này: Y điểm (tăng/giảm Z điểm)"

**Trường hợp ngoại lệ:**
1. Nếu chưa có lần khám trước → Không hiển thị phần so sánh
2. Nếu chỉ trả lời một phần câu hỏi → Hiển thị ghi chú: "Dựa trên N/12 câu trả lời"

**Trường hợp lỗi:**
1. Khi không tải được dữ liệu lần khám trước → Hệ thống vẫn hiển thị chi tiết lần hiện tại, ẩn phần so sánh

### Ghi chú kỹ thuật
- Sử dụng Popover hoặc Dialog cho chi tiết OSDI
- Progress bar với gradient 4 vùng màu (green → yellow → orange → red)
- Dữ liệu nhóm câu hỏi tính client-side từ câu trả lời đã lưu

---

## US-DRY-005: Biểu đồ xu hướng OSDI qua các lần khám

**Là một** bác sĩ, **Tôi muốn** xem biểu đồ xu hướng điểm OSDI của bệnh nhân qua nhiều lần khám, **Để** đánh giá hiệu quả điều trị và tiến triển bệnh mắt khô theo thời gian.

**Yêu cầu liên quan:** DRY-03

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Bác sĩ mở hồ sơ bệnh nhân và chọn tab "Lịch sử mắt khô" hoặc biểu đồ trong phần đánh giá mắt khô → Hệ thống hiển thị biểu đồ đường (line chart) với:
   - Trục X: Ngày khám (các lần khám)
   - Trục Y: Điểm OSDI (0-100)
   - Đường kẻ ngang phân vùng mức độ: 12 (Bình thường/Nhẹ), 22 (Nhẹ/Trung bình), 32 (Trung bình/Nặng) với mã màu tương ứng
2. Bác sĩ di chuột vào điểm trên biểu đồ → Hệ thống hiển thị tooltip: ngày khám, điểm OSDI, mức độ
3. Biểu đồ hiển thị mặc định tất cả các lần khám có dữ liệu OSDI

**Trường hợp ngoại lệ:**
1. Nếu bệnh nhân chỉ có 1 lần khám → Hiển thị biểu đồ với 1 điểm dữ liệu
2. Nếu bệnh nhân chưa có dữ liệu OSDI → Hiển thị thông báo: "Chưa có dữ liệu OSDI để hiển thị biểu đồ"
3. Nếu có khoảng trống giữa các lần khám (bệnh nhân bỏ tái khám) → Biểu đồ vẫn nối liền các điểm, trục X phản ánh khoảng thời gian thực

**Trường hợp lỗi:**
1. Khi không tải được dữ liệu lịch sử → Hệ thống hiển thị thông báo lỗi và nút thử lại
2. Khi dữ liệu OSDI bị hỏng (giá trị ngoài 0-100) → Hệ thống bỏ qua điểm dữ liệu lỗi, hiển thị cảnh báo

### Ghi chú kỹ thuật
- Sử dụng Recharts (MIT, miễn phí) cho biểu đồ xu hướng
- Biểu đồ đường (LineChart) với ReferenceLine tại y=12, y=22, y=32 cho phân vùng mức độ
- Vùng nền fill 4 màu giữa các đường tham chiếu (ReferenceArea)
- API endpoint: GET /api/clinical/patients/{patientId}/osdi-history
- Responsive cho cả desktop và tablet

---

## US-DRY-006: So sánh chỉ số mắt khô giữa các lần khám

**Là một** bác sĩ, **Tôi muốn** so sánh các chỉ số mắt khô (TBUT, Schirmer, phân độ tuyến Meibomius, bờ mi nước mắt, điểm nhuộm) giữa hai lần khám cạnh nhau, **Để** đánh giá cụ thể sự thay đổi của từng chỉ số sau điều trị.

**Yêu cầu liên quan:** DRY-04

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Bác sĩ mở chức năng so sánh lần khám → Hệ thống hiển thị 2 dropdown chọn lần khám (mặc định: lần gần nhất và lần kế gần nhất)
2. Bác sĩ chọn 2 lần khám cần so sánh → Hệ thống hiển thị bảng so sánh song song (side-by-side) với các chỉ số:
   - OSDI score (với mức độ và mã màu)
   - TBUT OD/OS (giây)
   - Schirmer OD/OS (mm)
   - Phân độ tuyến Meibomius OD/OS (0-3)
   - Bờ mi nước mắt OD/OS (mm)
   - Điểm nhuộm OD/OS
3. Hệ thống hiển thị mũi tên tăng/giảm và phần trăm thay đổi cho mỗi chỉ số → Bác sĩ đánh giá nhanh chiều hướng
4. Các chỉ số cải thiện hiển thị màu xanh, xấu đi hiển thị màu đỏ

**Trường hợp ngoại lệ:**
1. Nếu bệnh nhân chỉ có 1 lần khám → Hệ thống hiển thị thông báo: "Cần ít nhất 2 lần khám để so sánh"
2. Nếu một lần khám thiếu một số chỉ số → Hiển thị "N/A" cho chỉ số không có dữ liệu
3. Nếu giá trị giống nhau giữa 2 lần → Hiển thị "Không đổi" với biểu tượng ngang

**Trường hợp lỗi:**
1. Khi không tải được dữ liệu so sánh → Hệ thống hiển thị thông báo lỗi
2. Khi chọn cùng một lần khám cho cả hai → Hệ thống ngăn chặn và hiển thị: "Vui lòng chọn hai lần khám khác nhau"

### Ghi chú kỹ thuật
- Bảng so sánh có thể đặt trong patient profile tab hoặc visit detail overlay
- Chỉ số "cải thiện" phụ thuộc loại: TBUT tăng = tốt, Staining giảm = tốt, Meibomian grade giảm = tốt
- API endpoint: GET /api/clinical/patients/{patientId}/dry-eye-comparison?visitId1=...&visitId2=...
- Sử dụng cùng bệnh nhân, cùng loại dữ liệu, khác lần khám

---

## US-DRY-007: Bệnh nhân tự điền bộ câu hỏi OSDI qua QR code

**Là một** bệnh nhân, **Tôi muốn** tự điền bộ câu hỏi OSDI trên điện thoại qua quét mã QR mà không cần đăng nhập, **Để** tiết kiệm thời gian khám và trả lời thoải mái hơn.

**Yêu cầu liên quan:** DRY-01

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Nhân viên tạo mã QR cho bệnh nhân từ màn hình lượt khám → Hệ thống sinh mã QR chứa link công khai (public) với token duy nhất
2. Bệnh nhân quét mã QR bằng điện thoại → Trình duyệt mở trang điền OSDI (không cần đăng nhập)
3. Trang hiển thị 12 câu hỏi OSDI đầy đủ tiếng Việt với giao diện thân thiện mobile → Bệnh nhân chọn câu trả lời cho từng câu
4. Bệnh nhân nhấn "Gửi" → Hệ thống lưu kết quả và đồng bộ điểm OSDI về lượt khám
5. Bác sĩ nhìn thấy kết quả OSDI đã cập nhật trong phần đánh giá mắt khô (không cần reload trang)

**Trường hợp ngoại lệ:**
1. Nếu bệnh nhân chỉ trả lời một phần câu hỏi → Hệ thống tính OSDI dựa trên số câu đã trả lời
2. Nếu bệnh nhân quét lại mã QR → Hệ thống hiển thị câu trả lời đã điền trước đó, cho phép chỉnh sửa
3. Nếu bệnh nhân gửi nhiều lần → Hệ thống cập nhật kết quả mới nhất

**Trường hợp lỗi:**
1. Khi token hết hạn (sau 24 giờ) → Hệ thống hiển thị: "Liên kết đã hết hạn. Vui lòng yêu cầu mã QR mới từ nhân viên"
2. Khi token không hợp lệ → Hệ thống hiển thị: "Liên kết không hợp lệ"
3. Khi mất kết nối khi gửi → Hệ thống lưu tạm (local storage) và thử lại khi có kết nối

### Ghi chú kỹ thuật
- Trang công khai (public page) không cần xác thực, tương tự pattern self-booking (Phase 2)
- API endpoint: POST /api/public/osdi/{token} - không yêu cầu RequireAuthorization, có RequireRateLimiting
- Token duy nhất (unique) liên kết với visitId, hết hạn sau 24 giờ
- Giao diện mobile-first cho trang bệnh nhân tự điền
- Đồng bộ kết quả qua polling hoặc SignalR (bác sĩ nhận kết quả realtime)

---

## US-DRY-008: Nhân viên tạo mã QR cho bệnh nhân điền OSDI

**Là một** nhân viên phòng khám, **Tôi muốn** tạo và in mã QR cho bệnh nhân tự điền OSDI, **Để** giảm thời gian khám và bệnh nhân có thể trả lời trong khi chờ đợi.

**Yêu cầu liên quan:** DRY-01

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Nhân viên mở lượt khám và nhấn "Tạo QR OSDI" → Hệ thống tạo token duy nhất và sinh mã QR
2. Hệ thống hiển thị mã QR trên màn hình với các tùy chọn:
   - In mã QR (cho bệnh nhân quét)
   - Hiển thị mã QR lớn để bệnh nhân quét trực tiếp từ màn hình
   - Sao chép link để gửi qua Zalo/SMS
3. Nhân viên đưa mã QR cho bệnh nhân → Bệnh nhân quét và bắt đầu điền (Xem thêm: US-DRY-007)
4. Trên màn hình lượt khám, trạng thái OSDI cập nhật: "Đang chờ bệnh nhân điền" → "Đã nhận kết quả" khi bệnh nhân gửi

**Trường hợp ngoại lệ:**
1. Nếu tạo QR nhiều lần → Token cũ bị vô hiệu, chỉ token mới nhất hoạt động
2. Nếu lượt khám đã ký duyệt → Vẫn cho phép tạo QR (OSDI là dữ liệu bổ sung, không thay đổi hồ sơ đã ký)

**Trường hợp lỗi:**
1. Khi tạo token thất bại → Hệ thống hiển thị toast lỗi: "Không thể tạo mã QR. Vui lòng thử lại"
2. Khi in thất bại → Hệ thống fallback hiển thị QR lớn trên màn hình để bệnh nhân quét trực tiếp

### Ghi chú kỹ thuật
- Sử dụng thư viện QR code miễn phí (ví dụ: qrcode.react, MIT license)
- Token lưu trong database, liên kết với visitId, hết hạn 24 giờ
- Trạng thái polling: chưa mở / đang điền / đã gửi
- API endpoint: POST /api/clinical/visits/{visitId}/osdi-token (tạo token)
- API endpoint: GET /api/clinical/visits/{visitId}/osdi-status (kiểm tra trạng thái)
