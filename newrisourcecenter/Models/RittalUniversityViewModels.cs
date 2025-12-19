using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace newrisourcecenter.Models
{
    [Table("stats_training")]
    public partial class RittalUniversityViewModels
    {
        [Key]
        [Display(Name ="Test Id")]
        public int trid { get; set; }
        [Display(Name = "User Id")]
        public Nullable<long> tr_usr { get; set; }
        [Display(Name = "Date Created")]
        public Nullable<System.DateTime> tr_date { get; set; }
        [Display(Name = "Class Name")]
        public string tr_module { get; set; }
        [Display(Name = "Number of Questions")]
        public string tr_NumQuestions { get; set; }
        [Display(Name = "Passing Grade")]
        public string tr_PassGrade { get; set; }
        [Display(Name = "Score")]
        public string tr_score { get; set; }
        [Display(Name = "UserName")]
        [NotMapped]
        public string UserName { get; set; }
        [NotMapped]
        public List<Nav2ViewModel> list_courses { get; set; }
        [NotMapped]
        public List<Nav3ViewModel> list_classes { get; set; }
        [NotMapped]
        public List<SalesCommViewModel> list_announcements { get; set; }
        [NotMapped]
        public string usr_email { get; set; }
        [NotMapped]
        public Dictionary<string,List<trainingClass>> trainingClasses { get; set; }

        [NotMapped]
        public List<trainingTrack> trainingTracks { get; set; }
        [NotMapped]
        public Dictionary<int,usrClass> usrClasses { get; set; }
        [NotMapped]
        public List<string> trainingTrackNames { get; set; }
    }

    public class Uniclasses
    {
        public long? id { get; set; }
        public string name { get; set; }
    }

}