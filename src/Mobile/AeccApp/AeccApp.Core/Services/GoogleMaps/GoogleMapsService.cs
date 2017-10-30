﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using AeccApp.Core.Models;
using System.Globalization;

namespace AeccApp.Core.Services
{
    public class GoogleMapsService : IGoogleMapsService
    {
        private readonly IHttpRequestProvider _requestProvider;
        private const string GOOGLE_MAPS_ENDPOINT = "https://maps.googleapis.com";

        public GoogleMapsService(IHttpRequestProvider requestProvider)
        {
            _requestProvider = requestProvider;
        }

        public async Task<IEnumerable<AddressModel>> FindPlacesAsync(string findText, Position position)
        {
            string query = $"input={findText}&language=es&components=country:es&key={GlobalSetting.Instance.GooglePlacesApiKey}";
            if (position?.Latitude != 0)
            {
                query += $"&location={position.Latitude.ToString(CultureInfo.InvariantCulture)},{position.Longitude.ToString(CultureInfo.InvariantCulture)}";
            }

            UriBuilder uriBuilder = new UriBuilder(GOOGLE_MAPS_ENDPOINT)
            {
                Path = "maps/api/place/autocomplete/json",
                Query = query
            };

            var places = await _requestProvider.GetAsync<GooglePlacesModel>(uriBuilder.ToString());

            if (places?.Predictions != null)
                return places.Predictions.Select(item => new AddressModel(item));
            else
                return new List<AddressModel>();
        }

        public async Task<AddressModel> FillPlaceDetailAsync(AddressModel address)
        {
            UriBuilder uriBuilder = new UriBuilder(GOOGLE_MAPS_ENDPOINT)
            {
                Path = "maps/api/place/details/json",
                Query = $"placeid={address.PlaceId}&language=es&key={GlobalSetting.Instance.GooglePlacesApiKey}"
            };

            GooglePlacesDetailModel place = await _requestProvider.GetAsync<GooglePlacesDetailModel>(uriBuilder.ToString());
            address.AddCoordinates(place);
            return address;
        }

        public async Task<Position> FindAddressGeocodingAsync(string address)
        {
            UriBuilder uriBuilder = new UriBuilder(GOOGLE_MAPS_ENDPOINT)
            {
                Path = "maps/api/geocode/json",
                Query = $"address={address}&language=es&key={GlobalSetting.Instance.GooglePlacesApiKey}"
            };

            GoogleGeocodingModel place = await _requestProvider.GetAsync<GoogleGeocodingModel>(uriBuilder.ToString());
            var result = place.Results.FirstOrDefault();

            return (result != null) ?
                new Position(result.Geometry.Location.Lat, result.Geometry.Location.Lng) : null;
        }

        public async Task<AddressModel> FindCoordinatesGeocodingAsync(double lat, double lng)
        {
            UriBuilder uriBuilder = new UriBuilder(GOOGLE_MAPS_ENDPOINT)
            {
                Path = "maps/api/geocode/json",
                Query = $"latlng={lat.ToString(CultureInfo.InvariantCulture)},{lng.ToString(CultureInfo.InvariantCulture)}&language=es&key={GlobalSetting.Instance.GooglePlacesApiKey}"
            };

            GoogleGeocodingModel place = await _requestProvider.GetAsync<GoogleGeocodingModel>(uriBuilder.ToString());

            var result = place.Results.FirstOrDefault();

            return (result != null) ?
                new AddressModel()
                {
                    Province = result.AddressComponents.FirstOrDefault(c => c.Types.First().StartsWith("administrative_area_level"))?.LongName
                } : null;
        }
    }
}
