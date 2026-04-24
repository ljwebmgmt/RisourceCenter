using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace newrisourcecenter.Models
{
    public class QuizSubmissionViewModel
    {
        public int ContentId { get; set; }
        public List<QuestionAnswer> Answers { get; set; }
    }

    public class QuestionAnswer
    {
        public int QuestionId { get; set; }
        public List<int> SelectedOptionIds { get; set; }
    }
}