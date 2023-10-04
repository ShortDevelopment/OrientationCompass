using Android.Content;
using Android.Content.PM;
using Android.Hardware;
using Android.OS;
using Android.Runtime;
using Android.Views.Animations;
using AndroidX.AppCompat.App;
using AndroidX.Core.App;
using Google.Android.Material.Color;
using System.Diagnostics;

[assembly: UsesPermission(Android.Manifest.Permission.Vibrate)]

namespace OrientationCompass;

[Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
public class MainActivity : AppCompatActivity, ISensorEventListener
{
    ImageView _image = null!;
    Vibrator? _vibrator;
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        DynamicColors.ApplyToActivityIfAvailable(this);

        // Set our view from the "main" layout resource
        SetContentView(Resource.Layout.activity_main);

        _image = FindViewById<ImageView>(Resource.Id.NeedleImageView)!;

        var sensorManager = (SensorManager?)GetSystemService(Context.SensorService) ?? throw new InvalidOperationException("Could not get sensor service");
        var sensor = sensorManager.GetDefaultSensor(SensorType.Orientation) ?? throw new InvalidOperationException("Could not get sensor");
        sensorManager.RegisterListener(this, sensor, SensorDelay.Ui);
        // ToDo: Cleanup

        ActivityCompat.RequestPermissions(this, new[] { Android.Manifest.Permission.Vibrate }, 0);
    }

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
    {
        if (grantResults.Any(x => x == Permission.Denied))
            return;

        _vibrator = TryGetVibrator();
    }

    public void OnAccuracyChanged(Sensor? sensor, [GeneratedEnum] SensorStatus accuracy)
    {

    }

    float _currentAngle = 0;
    public void OnSensorChanged(SensorEvent? e)
    {
        float degrees = MathF.Round(e?.Values?[0] ?? 0);
        RotateAnimation animation = new(
                _currentAngle,
                -degrees,
                /* pivotX */ Dimension.RelativeToSelf, 0.5f,
                /* pivotY */ Dimension.RelativeToSelf, 0.5f
        )
        {
            Duration = 210,
            FillAfter = true
        };

        System.Diagnostics.Debug.Print(degrees.ToString());

        Vibrate(_vibrator, degrees);

        _image.StartAnimation(animation);
        _currentAngle = -degrees;
    }

    Vibrator? TryGetVibrator()
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(31))
            return (Vibrator?)GetSystemService(Context.VibratorService);

        var manager = (VibratorManager?)GetSystemService(Context.VibratorManagerService);
        return manager?.DefaultVibrator;
    }

    static void Vibrate(Vibrator? vibrator, float angle)
    {
        const int threshold = 10;
        if (angle > threshold && angle < 360 - threshold)
            return;

        if (vibrator == null)
            return;

        if (OperatingSystem.IsAndroidVersionAtLeast(26))
        {
            vibrator.Vibrate(VibrationEffect.CreateOneShot(100, 255));
        }
        else
        {
            vibrator.Vibrate(100);
        }
    }
}