using System;
using System.Linq;
using Bux.Dbo;
using Bux.Dbo.Model;
using Serilog;

namespace Bux.Cpx
{
    public class CpxService
    {
        public static string CLASS_NAME = typeof(CpxService).Name;

        private readonly Db db;

        public CpxService(Db db)
        {
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
                var existing = db.CpxOfferWallCallback.FirstOrDefault(c => c.TransactionId == callbackData.TransactionId);
                if (existing != null)
                {
                    var wasUpdated = false;

                    if (callbackData.Status.HasValue && callbackData.Status != existing.Status)
                    {
                        existing.Status = callbackData.Status;
                        wasUpdated = true;
                    }

                    if (!string.IsNullOrWhiteSpace(callbackData.Type) && callbackData.Type != existing.Type)
                    {
                        existing.Type = callbackData.Type;
                        wasUpdated = true;
                    }

                    if (callbackData.AmountLocal.HasValue && callbackData.AmountLocal != existing.AmountLocal)
                    {
                        existing.AmountLocal = callbackData.AmountLocal;
                        wasUpdated = true;
                    }

                    if (callbackData.AmountUsd.HasValue && callbackData.AmountUsd != existing.AmountUsd)
                    {
                        existing.AmountUsd = callbackData.AmountUsd;
                        wasUpdated = true;
                    }

                    if (!string.IsNullOrWhiteSpace(callbackData.Hash) && callbackData.Hash != existing.Hash)
                    {
                        existing.Hash = callbackData.Hash;
                        wasUpdated = true;
                    }

                    if (!string.IsNullOrWhiteSpace(callbackData.IpClick) && callbackData.IpClick != existing.IpClick)
                    {
                        existing.IpClick = callbackData.IpClick;
                        wasUpdated = true;
                    }

                    if (!string.IsNullOrWhiteSpace(callbackData.SubId1) && callbackData.SubId1 != existing.SubId1)
                    {
                        existing.SubId1 = callbackData.SubId1;
                        wasUpdated = true;
                    }

                    if (!string.IsNullOrWhiteSpace(callbackData.SubId2) && callbackData.SubId2 != existing.SubId2)
                    {
                        existing.SubId2 = callbackData.SubId2;
                        wasUpdated = true;
                    }

                    if (!string.IsNullOrWhiteSpace(callbackData.UserId) && callbackData.UserId != existing.UserId)
                    {
                        existing.UserId = callbackData.UserId;
                        wasUpdated = true;
                    }

                    if (!string.IsNullOrWhiteSpace(callbackData.OfferId) && callbackData.OfferId != existing.OfferId)
                    {
                        existing.OfferId = callbackData.OfferId;
                        wasUpdated = true;
                    }

                    if (wasUpdated)
                    {
                        existing.RespondedAt = now;
                        existing.ResponseStatusCode = httpStatus;
                        existing.ResponseBody = "updated";
                        db.SaveChanges();
                        return new ProcessOfferWallCallbackReturn(httpStatus, "updated");
                    }

                    Log.Warning($"{CLASS_NAME}:{METHOD_NAME}: duplicate transaction: {callbackData.TransactionId}");
                    return new ProcessOfferWallCallbackReturn(httpStatus, "duplicate");
                }
            }

            var callback = new CpxOfferWallCallback
            {
                Status = callbackData.Status,
                TransactionId = callbackData.TransactionId,
                UserId = callbackData.UserId,
                SubId1 = callbackData.SubId1,
                SubId2 = callbackData.SubId2,
                AmountLocal = callbackData.AmountLocal,
                AmountUsd = callbackData.AmountUsd,
                OfferId = callbackData.OfferId,
                Hash = callbackData.Hash,
                IpClick = callbackData.IpClick,
                Type = callbackData.Type,
                ReceivedAt = now,
                RespondedAt = now,
                ResponseStatusCode = httpStatus,
                ResponseBody = "ok"
            };

            db.CpxOfferWallCallback.Add(callback);
            db.SaveChanges();

            return new ProcessOfferWallCallbackReturn(httpStatus, "ok");
        }
    }
}
