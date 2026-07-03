using System.Collections.Generic;
using System.Web;

namespace newrisourcecenter.Models
{
    public class TrainingCreateViewModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string TrainingClass { get; set; }
        public int PassingPercentage { get; set; }
        public short SortOrder { get; set; }
        public bool IsActive { get; set; }
        public HttpPostedFileBase PdfFile { get; set; }
        public HttpPostedFileBase VideoFile { get; set; }
        public List<string> RoleIds { get; set; }
        public List<TrainingQuestionCreateViewModel> Questions { get; set; }
    }

    public class TrainingTrackSaveViewModel
    {
        public short? Id { get; set; }
        public string Name { get; set; }
        public short SortOrder { get; set; }
        public short CompleteDays { get; set; }
        public string Prize { get; set; }
        public string ImageUrl { get; set; }
        public HttpPostedFileBase ImageFile { get; set; }
        public bool RemoveImage { get; set; }
    }

    public class TrainingTrackListItemViewModel
    {
        public short Id { get; set; }
        public string Name { get; set; }
        public short SortOrder { get; set; }
        public short CompleteDays { get; set; }
        public string Prize { get; set; }
        public string ImageUrl { get; set; }
    }

    public class TrainingUpdateViewModel : TrainingCreateViewModel
    {
        public int Id { get; set; }
    }

    public class TrainingQuestionCreateViewModel
    {
        public string QuestionText { get; set; }
        public List<TrainingOptionCreateViewModel> Options { get; set; }
    }

    public class TrainingOptionCreateViewModel
    {
        public string OptionText { get; set; }
        public bool IsCorrect { get; set; }
    }
}
