using Bux.Dbo;

namespace Bux.Aye
{
    public class AyeService
    {
        private Db db;
        public AyeService(Db db)
        {
            this.db = db;
        }

		public record ProcessOfferWallCallbackReturn(int httpStatus, string responseText);

        public ProcessOfferWallCallbackReturn ProcessOfferWallCallback(OfferWallCallbackData callbackData) 
		{ 
			return new ProcessOfferWallCallbackReturn(200, "ok"); 
		}
    }
}
