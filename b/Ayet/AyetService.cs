using Microsoft.EntityFrameworkCore;
using Bux.Dbo;
using Bux.Dbo.Model;
using System;
using System.Linq;
using Serilog;

namespace Bux.Ayet
{
    public class AyetService
    {
        public static string CLASS_NAME = typeof(AyetService).Name;

        private Db db;
        private int addSlotId;

        public AyetService(IConfiguration configuration, Db db)
        {
            if (configuration["ayet_offerwall_addslot_id"] == null)
            {
                throw new Exception("missing configuration value: ayet_offerwall_addslot_id");
            }
            this.addSlotId = configuration.GetValue<int>("ayet_offerwall_addslot_id");
            this.db = db;
        }

		public record ProcessOfferWallCallbackReturn(int httpStatus, string responseText);

        public ProcessOfferWallCallbackReturn ProcessOfferWallCallback(OfferWallCallbackData callbackData) 
		{
            const string METHOD_NAME = "ProcessOfferWallCallback()";

            const int httpStatus = 200;
            var now = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(callbackData.TransactionId))
            {
                var existing = db.AyeOfferWallCallback.FirstOrDefault(c => c.TransactionId == callbackData.TransactionId);
                if (existing != null)
                {
                    Log.Warning($"{CLASS_NAME}:{METHOD_NAME}: duplicate transaction: {callbackData.TransactionId}");
                    return new ProcessOfferWallCallbackReturn(httpStatus, "duplicate");
                }
            }

            var callback = new AyetOfferWallCallback
            {
                TransactionId = callbackData.TransactionId,
                PayoutUsd = callbackData.PayoutUsd,
                CurrencyAmount = callbackData.CurrencyAmount,
                ExternalIdentifier = callbackData.ExternalIdentifier,
                UserId = callbackData.UserId,
                PlacementIdentifier = callbackData.PlacementIdentifier,
                AdslotId = callbackData.AdslotId,
                SubId = callbackData.SubId,
                Ip = callbackData.Ip,
                OfferId = callbackData.OfferId,
                OfferName = callbackData.OfferName,
                DeviceUuid = callbackData.DeviceUuid,
                DeviceMake = callbackData.DeviceMake,
                DeviceModel = callbackData.DeviceModel,
                AdvertisingId = callbackData.AdvertisingId,
                Sha1AndroidId = callbackData.Sha1AndroidId,
                Sha1Imei = callbackData.Sha1Imei,
                IsChargeback = callbackData.IsChargeback,
                ChargebackReason = callbackData.ChargebackReason,
                ChargebackDate = callbackData.ChargebackDate,
                EventName = callbackData.EventName,
                EventValue = callbackData.EventValue,
                TaskName = callbackData.TaskName,
                TaskUuid = callbackData.TaskUuid,
                CurrencyIdentifier = callbackData.CurrencyIdentifier,
                CurrencyConversionRate = callbackData.CurrencyConversionRate,
                ClickDate = callbackData.ClickDate,
                CallbackTs = callbackData.CallbackTs,
                Custom1 = callbackData.Custom1,
                Custom2 = callbackData.Custom2,
                Custom3 = callbackData.Custom3,
                Custom4 = callbackData.Custom4,
                Custom5 = callbackData.Custom5,
                ReceivedAt = now,
                RespondedAt = now,
                ResponseStatusCode = httpStatus,
                ResponseBody = "ok"
            };

            db.AyeOfferWallCallback.Add(callback);
            db.SaveChanges();

			return new ProcessOfferWallCallbackReturn(httpStatus, "ok"); 
		}

        public async Task<string> EnsureAyetUser(int userId)
        {
            const string METHOD_NAME = "EnsureAyetUser()";

            // check if AyetUser table has a record with the given AyetUserId
            var ayetUser = await db.AyetUser.FirstOrDefaultAsync(u => u.UserId == userId);
            if (ayetUser != null)
            {
                return ayetUser.AyetUserId;
            }

            string ayetUserId = Guid.NewGuid().ToString("N");
            int n = 0;
            while (await db.AyetUser.AnyAsync(u => u.AyetUserId == ayetUserId))
            {
                ayetUserId = Guid.NewGuid().ToString("N");
                ++n;
                if (n > 10)
                {
                    Log.Error($"{CLASS_NAME}:{METHOD_NAME}: failed to generate unique AyetUserId after {n} attempts");
                    throw new Exception("failed to generate unique AyetUserId");
                }
            }

            ayetUser = new AyetUser
            {
                UserId = userId,
                AyetUserId = ayetUserId
            };
            db.AyetUser.Add(ayetUser);

            await db.SaveChangesAsync();

            return ayetUser.AyetUserId;
        }

        public async Task<string> GetOfferWallAddSlotLink(int userId)
        {
            string ayetUserId = await EnsureAyetUser(userId);
            string link = $"https://offerwall.ayet.io/offers?adSlot={addSlotId}&externalIdentifier={ayetUserId}";
            return link;
        }
    }
}

