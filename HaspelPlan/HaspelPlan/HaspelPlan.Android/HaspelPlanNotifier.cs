using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace HaspelPlan.Droid
{
    [Service]
    public class HaspelPlanNotifier : Service
    {
        private INotificationManager notificationManager = DependencyService.Get<INotificationManager>();
        private static Context context = global::Android.App.Application.Context;

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            MessagingCenter.Send<object, string>(this, "HaspelPlanNotifier", "Notfies timetable-changes");
            if(Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O) RegisterForegroundServiceO(intent);
            startPushingNotifications();

            return StartCommandResult.Sticky;
        }

        void startPushingNotifications()
        {
            string title = $"Local Notification #";
            string message = $"You have now received notifications!";

            while(true)
            {
                notificationManager.SendNotification(title, message);
            }
        }

        void RegisterForegroundServiceO(Intent intent)
        {
            var pendingIntent = PendingIntent.GetActivity(context, 0, intent, PendingIntentFlags.UpdateCurrent);
            Notification notification = new NotificationCompat.Builder(this, "com.Lagerfeuer.HaspelPlan.Notifier")
                .SetOngoing(true)
                .SetContentTitle("HaspelPlan-Notifier")
                .SetContentText("Notifies about timetable-changes")
                .SetSmallIcon(Resource.Drawable.xamagonBlue)
                .SetContentIntent(pendingIntent)
                .SetOngoing(true)
                .Build();

            StartForeground(936, notification);
        }
    }
}