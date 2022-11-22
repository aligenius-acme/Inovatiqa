namespace Inovatiqa.Web.Models.News
{
    public partial class AddNewsCommentModel
    {
        public string CommentTitle { get; set; }

        public string CommentText { get; set; }

        public bool DisplayCaptcha { get; set; }
    }
}