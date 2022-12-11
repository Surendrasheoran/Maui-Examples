namespace MAP_MAUI;
#if ANDROID
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.Widget;
using Android.App;
using Kotlin.Properties;
using static Android.Icu.Text.Transliterator;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using static Android.Print.PrintAttributes;
#endif

#if IOS || MACCATALYST
using CoreLocation;
using MapKit;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Maps.Platform;
using UIKit;
using CoreGraphics;

#endif
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps.Handlers;
using Microsoft.Maui.Platform;
using System.Collections.ObjectModel;
using System.Net.NetworkInformation;
using Microsoft.Maui.Maps;
using System.Collections;
using System.Linq;

public partial class MainPage : ContentPage
{
    int count = 20;

    private ObservableCollection<CustomPin> MapPins { get; set; }

    public IEnumerable Positions => MapPins;

    public MainPage()
    {
        InitializeComponent();
        BindingContext = this;
        MapPins = new ObservableCollection<CustomPin>();
        ModifyEntry();
    }

    void ModifyEntry()
    {
        Microsoft.Maui.Maps.Handlers.MapPinHandler.ElementMapper.AppendToMapping("MyCustomization", (handler, view) =>
        {
            if (view is CustomPin)
            {
                var custompin = view as CustomPin;
#if ANDROID
                var mapHandler = (IMapHandler?)CustomMap1?.Handler;
                var googleMap = mapHandler?.Map;

                if (googleMap is not null)
                {
                    foreach (var pin in MapPins)
                    {
                        var markerWithIcon = new MarkerOptions();
                        markerWithIcon.SetPosition(new LatLng(pin.Location.Latitude, pin.Location.Longitude));
                        markerWithIcon.SetTitle(pin.Label);
                        markerWithIcon.SetSnippet(pin.Address);

                        if (pin is CustomPin cp)
                        {
                            BitmapDescriptor? bitmapDescriptor = BitmapDescriptorFactory.FromResource(pin.PinImage);
                            markerWithIcon.SetIcon(bitmapDescriptor);
                            markerWithIcon.SetRotation(pin.Angle);
                            googleMap.SetOnMarkerClickListener(new CustomMarkerClickListener(pin));
                        }
                        googleMap.AddMarker(markerWithIcon);
                    }
                }
#elif IOS || MACCATALYST

                var pin = (CustomPin)view;
                var image = UIImage.FromBundle("local");
                var annotation = new CustomAnnotation()
                {
                    Identifier = pin.Id,
                    Image = image,
                    Title = pin.Label,
                    Subtitle = pin.Address,
                    Coordinate = new CLLocationCoordinate2D(pin.Location.Latitude, pin.Location.Longitude),
                };

                var nativeMap = (MauiMKMapView?)CustomMap1.Handler?.PlatformView;
                if (nativeMap is not null)
                {
                    var customAnnotations = nativeMap.Annotations.OfType<CustomAnnotation>().Where(x => x.Identifier == annotation.Identifier).ToArray();
                    nativeMap.RemoveAnnotations(customAnnotations);

                    nativeMap.GetViewForAnnotation += GetViewForAnnotations;
                    nativeMap.SetCenterCoordinate(new CLLocationCoordinate2D(pin.Location.Latitude, pin.Location.Longitude), true);

                    nativeMap.AddAnnotation(annotation);
                }

#elif WINDOWS
               
#endif
            }
        });
    }

#if IOS || MACCATALYST
    private MKAnnotationView GetViewForAnnotations(MKMapView mapView, IMKAnnotation annotation)
    {
        MKAnnotationView? annotationView = null;

        if (annotation is CustomAnnotation customAnnotation)
        {
            annotationView = mapView.DequeueReusableAnnotation(customAnnotation.Identifier.ToString()) ??
                             new MKAnnotationView(annotation, customAnnotation.Identifier.ToString());
            UIImage image = customAnnotation.Image ?? UIImage.FromBundle("airplan");
            //    image.transform = CGAffineTransformMakeRotation([degreesToRadians: imageRotationFix]);
            annotationView.Image = image;
            annotationView.Transform = CGAffineTransform.MakeRotation(180);
            annotationView.BackgroundColor = UIColor.Blue;

            UITapGestureRecognizer gesture = new UITapGestureRecognizer();
            gesture.AddTarget(() => HandleDrag(gesture, annotation));
            annotationView.AddGestureRecognizer(gesture);
            //   annotationView.Hidden = false;
            annotationView.CanShowCallout = true;
        }

        return annotationView ?? new MKAnnotationView(annotation, null);
    }

    private void HandleDrag(UITapGestureRecognizer recognizer, IMKAnnotation annotation)
    {
        // If it's just began, cache the location of the image
        if (recognizer.State == UIGestureRecognizerState.Ended)
        {
            var annotationView = annotation as CustomAnnotation;
            var customPin = MapPins.FirstOrDefault(o => o.Id == annotationView.Identifier);
            Pin_IsPinSelectionChanged1(customPin, null);
        }

    }
#endif

    private void OnCounterClicked(object sender, EventArgs e)
    {
        count += 20;
        var pin = new CustomPin()
        {
            MarkerId = Guid.NewGuid(),

            Address = $"Address {count}",
            Location = new Location(10 + count, 10 + count),
            Label = $"Name {count}",
            PinImage = Resource.Drawable.local,
            Angle = count
        };
        pin.IsPinSelectionChanged += Pin_IsPinSelectionChanged1;
        MapPins.Add(pin);
        OnPropertyChanged(nameof(Positions));
        CustomMap1.MoveToRegion(new MapSpan(pin.Location, 20, 20));
        CustomMap1.Pins.Clear();
    }

    private void Pin_IsPinSelectionChanged1(object sender, EventArgs e)
    {
        var pin = sender as CustomPin;
        count += 30;
#if ANDROID
        pin.Angle += count;
        pin.PinImage = Resource.Drawable.airplan;
        var selectedIndex = MapPins.IndexOf(pin);
        MapPins.Remove(pin);
        MapPins.Insert(selectedIndex, pin);

        //   OnPropertyChanged(nameof(Positions));
        CustomMap1.MoveToRegion(new MapSpan(pin.Location, 10, 10));
#endif
#if IOS
        DisplayAlert("Clicked", "Pin image clicked on iOS", "Ok");
#endif
        CustomMap1.Pins.Clear();
    }
}


#if IOS || MACCATALYST
public class CustomAnnotation : MKPointAnnotation
{
    public Guid Identifier { get; set; }
    public UIImage? Image { get; set; }
}

public class CustomPinHandler : MapPinHandler
{
    protected override IMKAnnotation CreatePlatformElement() => new CustomAnnotation();
}

#endif

#if ANDROID
public class CustomMarkerClickListener : Java.Lang.Object, GoogleMap.IOnMarkerClickListener
{
    private readonly CustomPin bitmap;

    public CustomMarkerClickListener(CustomPin customPin)
    {
        this.bitmap = customPin;
    }

    public bool OnMarkerClick(Marker marker)
    {

        this.bitmap.IsSelected = true;
        //marker.HideInfoWindow();
        marker.Remove();
        //marker.SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.local));

        //marker.ShowInfoWindow();
        return true;
    }
}

#endif


