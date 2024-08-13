“Query Configuration Summary.”

Bộ truy vấn tổng hợp được cấu hình chính trong MongoDb được sử dụng để tạo ra các truy cập Agrregations trong MongoDb.
Điều này giúp tạo ra các bộ truy vấn trực tiếp được cấu hình bằng String tương tác hoặc lưu trữ trực tiếp trong Collection trên MongoDb.
Tạo lập các cấu hình có thể chứa các chức năng: (đây cũng là các từ khóa cần biết khi làm việc với MongoDb)
- $match: dùng để lọc các bản ghi theo một điều kiện nào đó.
- $project: dùng để chọn các trường dữ liệu cần lấy ra từ một bản ghi, hoặc thêm một trường dữ liệu mới vào bản ghi đó.
- $group: dùng để nhóm các bản ghi theo một hoặc nhiều trường dữ liệu, và thực hiện các phép toán như đếm, tổng, trung bình, tìm giá trị lớn nhất, nhỏ nhất, ...
- $loockup: from(colection) -> let(localField) -> pipeline[{operation: $expr, $in, $eq, $ne, ...}] -> as (field))
- $unwind: dùng để giải quyết các trường hợp mà một trường có giá trị là một mảng, và chúng ta muốn mỗi phần tử trong mảng đó trở thành một bản ghi riêng biệt.
- $concatArrays: dành cho việc tạo Combine các Doc trong việc join các Collection và không có cùng cấu trúc. (combine)
- $facet: dùng để thực hiện nhiều bộ truy vấn trên cùng một tập dữ liệu, và trả về kết quả của tất cả các bộ truy vấn đó trong một mảng.
- $sort: dùng để sắp xếp các bản ghi theo một hoặc nhiều trường dữ liệu.
- $limit: dùng để giới hạn số lượng bản ghi trả về.
- $skip: dùng để bỏ qua một số lượng bản ghi đầu tiên, và chỉ trả về các bản ghi sau số lượng bản ghi đó.
- $out: dùng để lưu kết quả của bộ truy vấn vào một Collection mới.
- $addFields: dùng để thêm một trường dữ liệu mới vào các bản ghi.
- $set: dùng để thay đổi giá trị của một trường dữ liệu.
- $unset: dùng để xóa một trường dữ liệu khỏi các bản ghi.
- $replaceRoot: dùng để thay thế toàn bộ cấu trúc của một bản ghi bằng cấu trúc của một trường dữ liệu khác.
- $bucket: dùng để phân loại các bản ghi vào các nhóm dựa trên giá trị của một trường dữ liệu.
---- Phần này có vẻ ít dùng hơn ----
- $replaceWith: dùng để thay thế toàn bộ cấu trúc của một bản ghi bằng một giá trị khác.
- $redact: dùng để ẩn các trường dữ liệu của các bản ghi dựa trên một điều kiện nào đó.
- $sample: dùng để lấy ngẫu nhiên một số bản ghi từ một Collection.
- $sortByCount: dùng để nhóm các bản ghi theo một trường dữ liệu, và sắp xếp các nhóm theo số lượng bản ghi trong mỗi nhóm.
- $graphLookup: dùng để tìm kiếm các bản ghi trong một Collection dựa trên mối quan hệ giữa các bản ghi đó.
- $bucketAuto: dùng để phân loại các bản ghi vào các nhóm dựa trên giá trị của một trường dữ liệu, và tự động xác định các giới hạn của các nhóm.


Bản V1 được thiết kế các toán tử được định nghĩa cơ bản như trên và cho phép lựa chọn những phần toán tử được quy ước trước
	- Nhược điểm lớn nhất có lẽ là việc chỉnh sửa sẽ khó khăn khi không có kiến trúc cây cụ thể 
	- Có lẽ cần sử dụng một kiến trúc mới giống như BasicComponent ở trên vậy, lúc đó, ta có thể quản lý được theo dạng cây, nhưng điều cần đổi 
	là BasicComponent hiện tại chưa hỗ trợ Array, cần một thiết kế khác và tuy nhiên vẫn cần kế thừa BsonValue để có thể sử dụng chung được với 
	các thành phần khác (quy ươc thiết kế gồm thêm, sửa, xóa toán tử), do trước đó các phần được thiết kế trong Link với BasicComponent được dùng 
	để lưu trữ các bộ liên kết và tất nhiên bằng thì cấu trúc độc lập và nếu có Parent thì không có một child cụ thể, nên cần một cấu trúc mới 
	được tùy chỉnh tối ưu cho việc truy cập và thay đổi trong cả Array và Json thông thường, kết hợp được với cả AutoAlias 