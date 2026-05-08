using FresherMisa2026.Entities.Extensions;
using System;
using System.ComponentModel.DataAnnotations;

namespace FresherMisa2026.Entities.Candidate
{
    [ConfigTable("Candidate", false, "")]
    public class Candidate : BaseModel
    {
        [Key]
        public Guid CandidateID { get; set; }

        [Display(Name = "File CV")]
        public string? CVFile { get; set; }

        [Display(Name = "Ảnh đại diện")]
        public string? Avatar { get; set; }

        [IRequired]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }

        [Display(Name = "Ngày sinh")]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Giới tính")]
        public string? Gender { get; set; }

        [Display(Name = "Thành phố")]
        public string? City { get; set; }

        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Quốc gia")]
        public string? Country { get; set; }

        [Display(Name = "Tỉnh/Thành phố")]
        public string? Province { get; set; }

        [Display(Name = "Phường/Xã")]
        public string? Ward { get; set; }

        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }

        [Display(Name = "Cấp độ")]
        public string? Level { get; set; }

        [Display(Name = "Nơi học")]
        public string? EducationPlace { get; set; }

        [Display(Name = "Chuyên ngành")]
        public string? Major { get; set; }

        [Display(Name = "Ngày ứng tuyển")]
        public DateTime? HiringDate { get; set; }

        [Display(Name = "Nguồn ứng viên")]
        public string? CandidateSource { get; set; }

        [Display(Name = "HR phụ trách")]
        public string? HRInCharge { get; set; }

        [Display(Name = "Cộng tác viên")]
        public string? Collaborator { get; set; }

        [Display(Name = "Đã thêm tham chiếu")]
        public bool IsReferenceAdded { get; set; }

        [Display(Name = "Công ty cũ")]
        public string? LastCompany { get; set; }

        [Display(Name = "Công ty đang làm")]
        public string? WorkCompany { get; set; }

        [Display(Name = "Ngày bắt đầu làm việc")]
        public DateTime? WorkStartDate { get; set; }

        [Display(Name = "Ngày kết thúc làm việc")]
        public DateTime? WorkEndDate { get; set; }

        [Display(Name = "Vị trí làm việc")]
        public string? WorkPosition { get; set; }

        [Display(Name = "Mô tả công việc")]
        public string? WorkDescription { get; set; }

        [Display(Name = "Chiến dịch tuyển dụng")]
        public string? HiringCampaign { get; set; }

        [Display(Name = "Vòng phỏng vấn")]
        public string? HiringRound { get; set; }

        [Display(Name = "Đánh giá")]
        public string? Rating { get; set; }

        [Display(Name = "Vị trí ứng tuyển")]
        public string? JobPosition { get; set; }

        [Display(Name = "Đã là nhân viên")]
        public bool IsEmployee { get; set; }

        [Display(Name = "Phòng ban")]
        public string? Department { get; set; }
    }
}
