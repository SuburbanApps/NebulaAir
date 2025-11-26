param(
    [string]$BaseUrl = "http://localhost:5223"
)

$ErrorActionPreference = "Stop"

function Step {
    param([string]$Text)
    Write-Host ""
    Write-Host "--------------------------------------------------" -ForegroundColor White
    Write-Host ("[STEP] {0}" -f $Text) -ForegroundColor Yellow
    Write-Host "--------------------------------------------------" -ForegroundColor White
}

function Info {
    param([string]$Text)
    Write-Host ("[INFO] {0}" -f $Text) -ForegroundColor Cyan
}

function Ok {
    param([string]$Text)
    Write-Host ("[ OK ] {0}" -f $Text) -ForegroundColor Green
}

function ErrorMsg {
    param([string]$Text)
    Write-Host ("[ERROR] {0}" -f $Text) -ForegroundColor Red
}

Write-Host "========================================" -ForegroundColor White
Write-Host " NebulaAir - Smoke Test API" -ForegroundColor Magenta
Write-Host "========================================" -ForegroundColor White
Write-Host ""

$apiBase = "$BaseUrl/api/v1"

# Paso 0: comprobar que el backend esta levantado
Step "Paso 0: Comprobar que el backend esta levantado"

$healthUrl = "$apiBase/flights"
Info ("Comprobando disponibilidad de la API en {0}" -f $healthUrl)

try {
    $null = Invoke-RestMethod -Method GET -Uri $healthUrl
    Ok "El backend responde correctamente. Continuamos con los tests."
}
catch {
    ErrorMsg "No se ha podido conectar con el backend en $BaseUrl"
    ErrorMsg "Asegurate de tener la API levantada con:"
    ErrorMsg "  cd src\\NebulaAir.Api"
    ErrorMsg ("  dotnet run --urls ""{0}""" -f $BaseUrl)
    exit 1
}

try {
    # 1) Obtener vuelos
    Step "Paso 1: Obtener lista de vuelos"

    $flightsUrl = "$apiBase/flights"
    Info "GET $flightsUrl"

    $flightsResponse = Invoke-RestMethod -Method GET -Uri $flightsUrl

    if (-not $flightsResponse -or $flightsResponse.Count -eq 0) {
        throw "No se han obtenido vuelos. Necesitamos al menos un vuelo en la BD."
    }

    $firstFlight = $flightsResponse[0]
    Ok ("Vuelos encontrados: {0}" -f $flightsResponse.Count)
    Info ("Usando FlightId={0}, Code={1}" -f $firstFlight.id, $firstFlight.code)

    # 2) Crear una reserva
    Step "Paso 2: Crear una reserva"

    $createBookingUrl = "$apiBase/bookings"
    Info "POST $createBookingUrl"

    $bookingBodyObject = @{
        flightId       = $firstFlight.id
        passengerName  = "Test User"
        passengerEmail = "test.user@nebulaair.local"
    }

    $bookingJson = $bookingBodyObject | ConvertTo-Json
    Info "Body JSON:"
    Info $bookingJson

    $bookingResponse = Invoke-RestMethod -Method POST -Uri $createBookingUrl -Body $bookingJson -ContentType "application/json"
    $bookingId = $bookingResponse.id

    if (-not $bookingId) {
        throw "POST /bookings no devolvio un id de reserva."
    }

    Ok ("Reserva creada con Id={0}" -f $bookingId)

    # 3) Obtener la reserva por Id
    Step "Paso 3: Obtener la reserva creada"

    $getBookingUrl = "$apiBase/bookings/$bookingId"
    Info "GET $getBookingUrl"

    $bookingGet = Invoke-RestMethod -Method GET -Uri $getBookingUrl

    if ($bookingGet.id -ne $bookingId) {
        throw "Id obtenido no coincide con el esperado."
    }

    Ok ("Reserva obtenida correctamente. PassengerName={0}" -f $bookingGet.passengerName)

    # 4) Comprobar que aparece en GET /bookings
    Step "Paso 4: Comprobar que la reserva aparece en el listado"

    $listBookingsUrl = "$apiBase/bookings"
    Info "GET $listBookingsUrl"

    $allBookings = Invoke-RestMethod -Method GET -Uri $listBookingsUrl

    $found = $false
    foreach ($b in $allBookings) {
        if ($b.id -eq $bookingId) {
            $found = $true
            break
        }
    }

    if (-not $found) {
        throw "La reserva no aparece en el listado."
    }

    Ok "La reserva aparece correctamente en GET /bookings"

    # 5) Eliminar la reserva
    Step "Paso 5: Eliminar la reserva"

    $deleteBookingUrl = "$apiBase/bookings/$bookingId"
    Info "DELETE $deleteBookingUrl"

    Invoke-RestMethod -Method DELETE -Uri $deleteBookingUrl

    Ok "Reserva eliminada correctamente."

    # 6) Comprobar que NO existe
    Step "Paso 6: Validar que la reserva ya no existe"

    try {
        Info "GET $getBookingUrl (esperamos error tras el borrado)"
        $null = Invoke-RestMethod -Method GET -Uri $getBookingUrl
        throw "La reserva sigue existiendo tras DELETE."
    }
    catch {
        Ok "GET /bookings/$bookingId falla correctamente. La reserva ya no existe."
    }

    Write-Host ""
    Write-Host "========================================" -ForegroundColor White
    Write-Host " TODOS LOS TESTS SE HAN COMPLETADO OK" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor White
    exit 0
}
catch {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor White
    Write-Host " ERROR DURANTE LA EJECUCION DE LOS TESTS" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor White

    ErrorMsg $_
    exit 1
}
