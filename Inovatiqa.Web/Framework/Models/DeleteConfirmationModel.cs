
namespace Inovatiqa.Web.Framework.Models
{
    public class DeleteConfirmationModel : BaseInovatiqaModel
    {
        public string Id { get; set; }

        public string ControllerName { get; set; }

        public string ActionName { get; set; }

        public string WindowId { get; set; }
    }
}