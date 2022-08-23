using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.Core.App;
using Android.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowsAzure.Messaging.NotificationHubs;
using AndroidApp = Android.App.Application;

namespace HaspelPlan.Droid
{
    public class AzureListener : Java.Lang.Object, INotificationListener
    {
        public void OnPushNotificationReceived(Context context, INotificationMessage message)
        {
            /*var intent = new Intent(this, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.ClearTop);
            var pendingIntent = PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.OneShot);
            
            var notificationBuilder = new AndroidX.Core.App.NotificationCompat.Builder(this, MainActivity.CHANNEL_ID);

            notificationBuilder.SetContentTitle(message.Title)
                        .SetSmallIcon(Resource.Drawable.ic_launcher)
                        .SetContentText(message.Body)
                        .SetAutoCancel(true)
                        .SetShowWhen(false)
                        .SetContentIntent(pendingIntent);

            var notificationManager = NotificationManager.FromContext(this);

            notificationManager.Notify(0, notificationBuilder.Build());
             
             */

            //
            int pendingIntentId = 0;
            const string TitleKey = "titleKey";
            const string title = "title";
            const string MessageKey = "messageKey";

            Intent intent2 = new Intent(AndroidApp.Context, typeof(MainActivity));
            intent2.PutExtra(TitleKey, title);
            intent2.PutExtra(MessageKey, message.ToString());

            PendingIntent pendingIntent = PendingIntent.GetActivity(AndroidApp.Context, pendingIntentId++, intent2, PendingIntentFlags.UpdateCurrent);

            NotificationCompat.Builder builder = new NotificationCompat.Builder(AndroidApp.Context, MainActivity.CHANNEL_ID)
                .SetContentIntent(pendingIntent)
                .SetContentTitle(title)
                .SetContentText(message.ToString())
                .SetLargeIcon(BitmapFactory.DecodeResource(AndroidApp.Context.Resources, Resource.Drawable.xamagonBlue))
                .SetSmallIcon(Resource.Drawable.xamagonBlue)
                .SetDefaults((int)NotificationDefaults.Sound | (int)NotificationDefaults.Vibrate);

            var notificationManager = NotificationManager.FromContext(AndroidApp.Context);

            notificationManager.Notify(0, builder.Build());
            //
        }

    }
}