# Hình Ảnh Y Khoa (Medical Imaging) - User Stories

**Phạm vi:** Quản lý hình ảnh y khoa bao gồm: tải lên hình ảnh chẩn đoán (Fluorescein, Meibography, OCT, Kính hiển vi phản chiếu, Bản đồ giác mạc), tải lên video (thủ thuật lệ đạo, v.v.), phân loại hình ảnh theo loại và mắt, xem hình ảnh trong lightbox với khả năng phóng to, so sánh hình ảnh giữa các lần khám, và quản lý hình ảnh (xóa, sắp xếp).
**Yêu cầu liên quan:** IMG-01, IMG-02, IMG-03, IMG-04
**Số lượng user stories:** 8

---

## US-IMG-001: Nhân viên tải lên hình ảnh y khoa cho lượt khám

**Là một** nhân viên phòng khám (kỹ thuật viên hoặc bác sĩ), **Tôi muốn** tải lên hình ảnh y khoa và liên kết với lượt khám đúng, có phân loại theo loại hình ảnh, **Để** lưu trữ kết quả chẩn đoán hình ảnh phục vụ theo dõi và so sánh.

**Yêu cầu liên quan:** IMG-01

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Nhân viên mở lượt khám và chọn phần "Hình ảnh y khoa" → Hệ thống hiển thị MedicalImagesSection dạng VisitSection (thu gọn được) với vùng tải lên và thư viện hình ảnh hiện có
2. Nhân viên nhấn "Tải lên hình ảnh" → Hệ thống hiển thị form tải lên với:
   - Chọn file (kéo thả hoặc click chọn), hỗ trợ chọn nhiều file cùng lúc
   - **Chọn loại hình ảnh (bắt buộc):** Fluorescein / Meibography / OCT / Kính hiển vi phản chiếu (Specular microscopy) / Bản đồ giác mạc (Topography)
   - **Chọn mắt (tùy chọn):** OD (Mắt phải) / OS (Mắt trái) / OU (Hai mắt) / Không chỉ định
3. Nhân viên chọn loại hình ảnh, tùy chọn gắn thẻ mắt, và xác nhận → Hệ thống tải lên Azure Blob Storage và hiển thị thumbnail trong thư viện
4. Hệ thống hiển thị thanh tiến trình (progress bar) trong quá trình tải lên → Hoàn thành hiển thị toast: "Đã tải lên N hình ảnh"

**Trường hợp ngoại lệ:**
1. Nếu lượt khám đã ký duyệt → Vẫn cho phép tải lên hình ảnh (append-only, không chỉnh sửa hồ sơ y tế)
2. Nếu tải lên nhiều file cùng lúc → Hệ thống xử lý song song với progress bar riêng cho từng file
3. Nếu hình ảnh không thuộc mắt cụ thể (ví dụ: ảnh khuôn mặt) → Nhân viên chọn "Không chỉ định" cho thẻ mắt

**Trường hợp lỗi:**
1. Khi file vượt quá giới hạn dung lượng (> 20MB cho ảnh) → Hệ thống từ chối và hiển thị: "Dung lượng file vượt quá giới hạn 20MB"
2. Khi định dạng file không được hỗ trợ → Hệ thống từ chối: "Chỉ hỗ trợ định dạng: JPEG, PNG, TIFF, BMP, DICOM preview"
3. Khi không chọn loại hình ảnh → Hệ thống hiển thị validation: "Vui lòng chọn loại hình ảnh"
4. Khi tải lên thất bại (lỗi mạng, lỗi server) → Hệ thống hiển thị toast lỗi: "Tải lên thất bại. Vui lòng thử lại"
5. Khi số lượng hình ảnh trên lượt khám vượt quá 50 → Hệ thống cảnh báo: "Đã đạt giới hạn hình ảnh cho lượt khám này"

### Ghi chú kỹ thuật
- Tải lên sử dụng FormData + raw fetch (không phải openapi-fetch) cho multipart file upload, tương tự Patient photo upload pattern
- Endpoint: POST /api/clinical/visits/{visitId}/images với DisableAntiforgery()
- Azure Blob Storage container "clinical-images" với soft delete và versioning (ARC-05)
- Loại hình ảnh là trường bắt buộc; thẻ mắt (OD/OS/OU) là tùy chọn
- Giới hạn file: tối đa 20MB cho ảnh, tối đa 50 file mỗi lượt khám
- Hỗ trợ định dạng: JPEG, PNG, TIFF, BMP
- SAS URL cho truy cập hình ảnh (bảo mật, có thời hạn)

---

## US-IMG-002: Nhân viên phân loại và gắn thẻ hình ảnh khi tải lên

**Là một** nhân viên phòng khám, **Tôi muốn** phân loại hình ảnh theo loại chẩn đoán và gắn thẻ mắt (phải/trái/hai mắt) khi tải lên, **Để** dễ dàng tìm kiếm, lọc và so sánh hình ảnh giữa các lần khám.

**Yêu cầu liên quan:** IMG-01

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Nhân viên chọn file hình ảnh để tải lên → Hệ thống hiển thị form phân loại cho từng file:
   - **Loại hình ảnh (bắt buộc):** dropdown với các tùy chọn: Fluorescein, Meibography, OCT, Kính hiển vi phản chiếu, Bản đồ giác mạc
   - **Thẻ mắt (tùy chọn):** OD (Mắt phải) / OS (Mắt trái) / OU (Hai mắt) / Không chỉ định
   - **Ghi chú (tùy chọn):** text ngắn mô tả bổ sung
2. Khi tải lên nhiều file cùng loại → Nhân viên có thể chọn loại hình ảnh chung cho tất cả file (áp dụng hàng loạt)
3. Hệ thống lưu phân loại cùng với hình ảnh → Hiển thị trong thư viện với nhãn loại và thẻ mắt

**Trường hợp ngoại lệ:**
1. Nếu chụp ảnh chung (không phải per-eye) → Nhân viên chọn "Không chỉ định" hoặc "OU" cho thẻ mắt
2. Nếu muốn thay đổi phân loại sau khi tải lên → Nhân viên có thể chỉnh sửa loại và thẻ mắt (nếu lượt khám chưa ký duyệt)

**Trường hợp lỗi:**
1. Khi không chọn loại hình ảnh → Hệ thống từ chối tải lên: "Vui lòng chọn loại hình ảnh"
2. Khi cập nhật phân loại thất bại → Hệ thống hiển thị toast lỗi

### Ghi chú kỹ thuật
- Image type enum: Fluorescein, Meibography, OCT, SpecularMicroscopy, Topography, Video
- Eye tag enum: OD, OS, OU, Unspecified (nullable/optional)
- Phân loại hình ảnh cho phép so sánh cùng loại giữa các lần khám (IMG-04)
- Metadata lưu trong database, file lưu Azure Blob Storage

---

## US-IMG-003: Nhân viên tải lên video y khoa cho lượt khám

**Là một** nhân viên phòng khám, **Tôi muốn** tải lên video y khoa (ví dụ: thủ thuật lệ đạo, khám qua đèn khe) và liên kết với lượt khám, **Để** lưu trữ bằng chứng hình ảnh động phục vụ theo dõi và hội chẩn.

**Yêu cầu liên quan:** IMG-02

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Nhân viên nhấn "Tải lên video" trong phần Hình ảnh y khoa → Hệ thống hiển thị form tải lên video với:
   - Chọn file video (hỗ trợ: MP4, MOV, AVI, WebM)
   - Chọn loại: "Video" (tự động chọn loại Video)
   - Tùy chọn gắn thẻ mắt (OD/OS/OU/Không chỉ định)
   - Ghi chú mô tả (tùy chọn)
2. Nhân viên xác nhận tải lên → Hệ thống hiển thị thanh tiến trình với phần trăm hoàn thành
3. Tải lên hoàn thành → Video xuất hiện trong thư viện với thumbnail preview (frame đầu tiên hoặc icon video)
4. Hệ thống hiển thị toast: "Đã tải lên video thành công"

**Trường hợp ngoại lệ:**
1. Nếu lượt khám đã ký duyệt → Vẫn cho phép tải lên video (append-only)
2. Nếu video dung lượng lớn → Hệ thống hiển thị thanh tiến trình chi tiết với tốc độ tải lên
3. Nếu mất kết nối giữa chừng → Hệ thống hiển thị: "Tải lên bị gián đoạn. Vui lòng thử lại"

**Trường hợp lỗi:**
1. Khi file vượt quá giới hạn dung lượng (> 200MB cho video) → Hệ thống từ chối: "Dung lượng video vượt quá giới hạn 200MB"
2. Khi định dạng video không hỗ trợ → Hệ thống từ chối: "Chỉ hỗ trợ định dạng: MP4, MOV, AVI, WebM"
3. Khi tải lên thất bại → Hệ thống hiển thị toast lỗi: "Tải lên video thất bại"

### Ghi chú kỹ thuật
- Giới hạn file video: tối đa 200MB
- Hỗ trợ định dạng: MP4, MOV, AVI, WebM
- Tải lên sử dụng FormData + raw fetch với progress tracking (XMLHttpRequest hoặc fetch với ReadableStream)
- Azure Blob Storage lưu trữ, SAS URL để phát lại video
- Thumbnail video: icon video placeholder (không cần tạo thumbnail thực từ frame)
- Video playback trong lightbox (Xem thêm: US-IMG-004)

---

## US-IMG-004: Bác sĩ xem hình ảnh trong lightbox với khả năng phóng to

**Là một** bác sĩ, **Tôi muốn** xem hình ảnh y khoa trong lightbox toàn màn hình với khả năng phóng to, thu nhỏ và điều hướng giữa các hình, **Để** kiểm tra chi tiết kết quả chẩn đoán hình ảnh.

**Yêu cầu liên quan:** IMG-03

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Bác sĩ nhấn vào thumbnail hình ảnh trong thư viện → Hệ thống mở lightbox toàn màn hình hiển thị hình ảnh gốc (full resolution)
2. Bác sĩ sử dụng các điều khiển lightbox:
   - **Phóng to/thu nhỏ:** cuộn chuột hoặc pinch-to-zoom (tablet), nút zoom +/-
   - **Kéo/di chuyển:** kéo hình ảnh khi đã phóng to
   - **Xoay:** nút xoay 90 độ (trái/phải)
   - **Điều hướng:** mũi tên trái/phải để xem hình tiếp theo/trước đó
   - **Đóng:** nhấn X hoặc nhấn Escape
3. Lightbox hiển thị thông tin hình ảnh: loại, thẻ mắt, ngày tải lên, ghi chú
4. Bác sĩ nhấn phím mũi tên hoặc nút điều hướng → Hệ thống chuyển sang hình ảnh tiếp theo trong cùng lượt khám

**Trường hợp ngoại lệ:**
1. Nếu hình ảnh dung lượng lớn → Hệ thống hiển thị spinner trong khi tải, hiển thị thumbnail trước rồi thay bằng hình gốc
2. Nếu chỉ có 1 hình ảnh → Ẩn nút điều hướng trái/phải
3. Nếu mở video → Lightbox hiển thị trình phát video (video player) thay vì hình ảnh tĩnh, hỗ trợ phát/tạm dừng/tua

**Trường hợp lỗi:**
1. Khi không tải được hình ảnh gốc → Hệ thống hiển thị thông báo lỗi trong lightbox: "Không thể tải hình ảnh"
2. Khi SAS URL hết hạn → Hệ thống tự động tạo SAS URL mới và tải lại hình ảnh

### Ghi chú kỹ thuật
- Sử dụng thư viện lightbox miễn phí (MIT license): yet-another-react-lightbox hoặc tương đương
- Lightbox hỗ trợ zoom, pan, rotate, slideshow
- Video playback trong lightbox sử dụng HTML5 video player
- Hình ảnh tải qua SAS URL (có thời hạn) từ Azure Blob Storage
- Responsive cho desktop và tablet
- Keyboard navigation: Arrow keys (điều hướng), Escape (đóng), +/- (zoom)

---

## US-IMG-005: Bác sĩ xem thư viện hình ảnh theo loại và mắt

**Là một** bác sĩ, **Tôi muốn** xem thư viện hình ảnh của lượt khám với bộ lọc theo loại hình ảnh và thẻ mắt, **Để** nhanh chóng tìm được hình ảnh cần xem trong số nhiều hình đã tải lên.

**Yêu cầu liên quan:** IMG-03

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Bác sĩ mở phần "Hình ảnh y khoa" trong lượt khám → Hệ thống hiển thị thư viện dạng lưới (grid) với thumbnail cho tất cả hình ảnh và video
2. Bác sĩ lọc theo loại hình ảnh → Hệ thống chỉ hiển thị hình ảnh thuộc loại đã chọn (ví dụ: chỉ OCT)
3. Bác sĩ lọc theo thẻ mắt → Hệ thống chỉ hiển thị hình ảnh của mắt đã chọn (OD/OS/OU)
4. Mỗi thumbnail hiển thị: nhãn loại hình ảnh, thẻ mắt (nếu có), ngày tải lên
5. Bác sĩ nhấn vào thumbnail → Mở lightbox (Xem thêm: US-IMG-004)

**Trường hợp ngoại lệ:**
1. Nếu lượt khám không có hình ảnh → Hiển thị thông báo: "Chưa có hình ảnh y khoa" với nút "Tải lên hình ảnh"
2. Nếu lọc không có kết quả → Hiển thị: "Không có hình ảnh [loại] cho [mắt]"
3. Nếu hình ảnh thuộc nhiều loại → Hiển thị tab hoặc nhóm theo loại hình ảnh

**Trường hợp lỗi:**
1. Khi không tải được thumbnail → Hệ thống hiển thị placeholder với icon lỗi
2. Khi SAS URL hết hạn → Hệ thống tự động refresh SAS URL

### Ghi chú kỹ thuật
- Thư viện hiển thị dạng grid responsive (2-4 cột tùy màn hình)
- Thumbnail load lazy (intersection observer) cho hiệu suất
- Lọc thực hiện client-side (dữ liệu đã tải về cùng lượt khám)
- Tab hoặc chip filter cho loại hình ảnh, tương tự RefractionSection tabbed pattern
- API endpoint: GET /api/clinical/visits/{visitId}/images (trả về danh sách với SAS URL)

---

## US-IMG-006: Bác sĩ so sánh hình ảnh giữa các lần khám

**Là một** bác sĩ, **Tôi muốn** so sánh hình ảnh y khoa cùng loại giữa hai lần khám cạnh nhau (side-by-side), **Để** đánh giá trực quan sự tiến triển hoặc cải thiện của bệnh.

**Yêu cầu liên quan:** IMG-04

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Bác sĩ mở chức năng so sánh hình ảnh → Hệ thống hiển thị giao diện so sánh với:
   - Chọn loại hình ảnh cần so sánh (ví dụ: Meibography)
   - Chọn lần khám thứ nhất (mặc định: lần khám hiện tại)
   - Chọn lần khám thứ hai (mặc định: lần khám gần nhất trước đó)
2. Bác sĩ chọn loại hình ảnh và hai lần khám → Hệ thống hiển thị hình ảnh song song (side-by-side):
   - Bên trái: hình ảnh lần khám thứ nhất với ngày khám
   - Bên phải: hình ảnh lần khám thứ hai với ngày khám
3. Bác sĩ có thể phóng to/thu nhỏ đồng bộ hai hình ảnh → Phóng to bên này, bên kia cũng phóng to cùng mức
4. Bác sĩ lọc theo thẻ mắt (OD/OS) → Hệ thống chỉ hiển thị hình ảnh của mắt tương ứng

**Trường hợp ngoại lệ:**
1. Nếu một lần khám không có hình ảnh cùng loại → Hiển thị placeholder: "Không có hình ảnh [loại] cho lần khám này"
2. Nếu một lần khám có nhiều hình ảnh cùng loại → Hiển thị carousel hoặc grid cho từng bên
3. Nếu bệnh nhân chỉ có 1 lần khám → Hiển thị thông báo: "Cần ít nhất 2 lần khám để so sánh hình ảnh"

**Trường hợp lỗi:**
1. Khi không tải được hình ảnh từ lần khám cũ → Hệ thống hiển thị thông báo lỗi cho bên đó
2. Khi SAS URL hết hạn → Hệ thống tự động tạo SAS URL mới

### Ghi chú kỹ thuật
- So sánh chỉ áp dụng cho cùng loại hình ảnh, cùng bệnh nhân, khác lần khám
- Giao diện side-by-side với synchronized zoom (zoom đồng bộ hai bên)
- Có thể triển khai dưới dạng full-screen overlay hoặc inline split view
- API endpoint: GET /api/clinical/patients/{patientId}/image-comparison?type=...&visitId1=...&visitId2=...
- Hình ảnh tải qua SAS URL từ Azure Blob Storage (bảo mật, có thời hạn)

---

## US-IMG-007: Bác sĩ so sánh hình ảnh với chỉ số mắt khô kết hợp

**Là một** bác sĩ, **Tôi muốn** xem hình ảnh so sánh kết hợp với chỉ số mắt khô tương ứng (TBUT, Schirmer, OSDI) từ cùng lần khám, **Để** có cái nhìn toàn diện về tiến triển bệnh dựa trên cả hình ảnh lẫn số liệu.

**Yêu cầu liên quan:** IMG-04

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Bác sĩ mở giao diện so sánh hình ảnh giữa hai lần khám → Hệ thống hiển thị hình ảnh song song (Xem thêm: US-IMG-006)
2. Bên dưới mỗi hình ảnh, hệ thống hiển thị bảng tóm tắt chỉ số mắt khô từ cùng lần khám:
   - OSDI score (với mức độ và mã màu)
   - TBUT OD/OS
   - Schirmer OD/OS
   - Phân độ tuyến Meibomius OD/OS
3. Bác sĩ nhìn thấy sự tương quan giữa thay đổi hình ảnh và thay đổi chỉ số → Đánh giá toàn diện hơn

**Trường hợp ngoại lệ:**
1. Nếu lần khám không có dữ liệu mắt khô → Hiển thị "Không có dữ liệu mắt khô" thay vì bảng chỉ số
2. Nếu chỉ có một phần chỉ số → Hiển thị chỉ số có dữ liệu, ẩn chỉ số không có

**Trường hợp lỗi:**
1. Khi không tải được dữ liệu mắt khô → Hệ thống vẫn hiển thị hình ảnh so sánh, ẩn phần chỉ số

### Ghi chú kỹ thuật
- Tích hợp dữ liệu từ DryEyeAssessment và MedicalImage của cùng lần khám
- Reuse bảng so sánh chỉ số từ US-DRY-006
- API có thể trả kết hợp hoặc frontend gọi song song 2 endpoint

---

## US-IMG-008: Nhân viên quản lý hình ảnh (xóa và sắp xếp)

**Là một** nhân viên phòng khám, **Tôi muốn** xóa hình ảnh tải lên nhầm và sắp xếp lại thứ tự hiển thị, **Để** đảm bảo thư viện hình ảnh chính xác và dễ sử dụng.

**Yêu cầu liên quan:** IMG-01

### Tiêu chí chấp nhận

**Luồng chính (Happy Path):**
1. Nhân viên nhấn nút xóa trên thumbnail hình ảnh → Hệ thống hiển thị AlertDialog xác nhận: "Bạn có chắc muốn xóa hình ảnh này? Thao tác không thể hoàn tác."
2. Nhân viên xác nhận xóa → Hệ thống xóa hình ảnh khỏi thư viện (soft delete trên Azure Blob Storage)
3. Hệ thống hiển thị toast: "Đã xóa hình ảnh"
4. Nhân viên có thể thay đổi thứ tự hiển thị hình ảnh bằng kéo thả → Hệ thống lưu thứ tự mới

**Trường hợp ngoại lệ:**
1. Nếu lượt khám đã ký duyệt → Chỉ cho phép xóa hình ảnh tải lên sau khi ký duyệt (hình ảnh trước ký duyệt không được xóa)
2. Nếu hình ảnh đang được sử dụng trong so sánh → Hiển thị cảnh báo: "Hình ảnh này đang được sử dụng trong so sánh. Bạn vẫn muốn xóa?"

**Trường hợp lỗi:**
1. Khi xóa thất bại → Hệ thống hiển thị toast lỗi: "Xóa hình ảnh thất bại"
2. Khi lưu thứ tự thất bại → Hệ thống hoàn tác vị trí và hiển thị toast lỗi

### Ghi chú kỹ thuật
- Xóa sử dụng soft delete trên Azure Blob Storage (ARC-05: soft delete + versioning)
- Xóa hình ảnh trước ký duyệt bị chặn nếu lượt khám đã ký
- AlertDialog (non-dismissible) cho xác nhận xóa
- API endpoint: DELETE /api/clinical/visits/{visitId}/images/{imageId}
- Kéo thả sắp xếp có thể sử dụng @dnd-kit (đã dùng cho Kanban board)
- Lưu thứ tự hiển thị (display order) trong database
