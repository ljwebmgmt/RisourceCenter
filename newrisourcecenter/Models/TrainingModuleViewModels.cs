using System.Collections.Generic;
using System.Web;

namespace newrisourcecenter.Models
{
    public class TrainingCreateViewModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int PassingPercentage { get; set; }
        public HttpPostedFileBase PdfFile { get; set; }
        public HttpPostedFileBase VideoFile { get; set; }
        public List<string> RoleIds { get; set; }
        public List<TrainingQuestionCreateViewModel> Questions { get; set; }
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
