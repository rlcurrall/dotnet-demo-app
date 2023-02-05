SELECT *
FROM Acme.WeatherForecast wf
WHERE wf.Date = :Date
  AND wf.Latitude = :Latitude
  AND wf.Longitude = :Longitude