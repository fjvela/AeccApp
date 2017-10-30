﻿using AeccApp.Core.Messages;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using Xamarin.Forms.Xaml;

namespace AeccApp.Core.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class CompletingHospitalRequestView : BaseContentPage
	{
		public CompletingHospitalRequestView ()
        {

            MessagingCenter.Subscribe<GeolocatorMessages, Position>(this, string.Empty, (sender, arg) => 
                MoveCameraMap(arg)
            );
            InitializeComponent();
            map.InitialCameraUpdate = CameraUpdateFactory.NewPositionZoom(new Position(40.416937, -3.703523), 6d);
           
        }

     
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MessagingCenter.Unsubscribe<GeolocatorMessages>(this, string.Empty);
        }
        public async void MoveCameraMap(Position toPosition)
        {
            var pinHospital = new Pin() { Label = "Tu selección", Position = toPosition };
            pinHospital.IsDraggable = false;
            switch (Device.OS)
            {
                case TargetPlatform.Android:
                    //   pinBravent.Icon = BitmapDescriptorFactory.FromBundle($"location_pin.png");
                    break;
                case TargetPlatform.iOS:
                    pinHospital.Icon = BitmapDescriptorFactory.FromBundle($"location_pin.png");
                    break;
                default:
                    pinHospital.Icon = BitmapDescriptorFactory.FromBundle($"Assets/location_pin.png");
                    break;
            }

            map.Pins.Add(pinHospital);
            var animState = await map.AnimateCamera(CameraUpdateFactory.NewPositionZoom(
                     toPosition, 16d), TimeSpan.FromSeconds(1));
        }
    }
}