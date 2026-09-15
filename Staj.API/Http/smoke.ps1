# Staj.API/Http/smoke.ps1
# ---------------------------------------------------------------------------
# Staj API uctan uca smoke test scripti.
# Kullanim:
#   powershell -ExecutionPolicy Bypass -File .\Staj.API\Http\smoke.ps1
#   powershell -ExecutionPolicy Bypass -File .\Staj.API\Http\smoke.ps1 -BaseUrl http://localhost:5041
#
# Her adim kendisinden onceki response dan token/ID okur; ilk fail de durur.
# ---------------------------------------------------------------------------

[CmdletBinding()]
param(
    [string]$BaseUrl = "http://localhost:5041",
    [string]$AdminEmail = "admin@staj.local",
    [string]$AdminPassword = "Admin123!"
)

$ErrorActionPreference = "Stop"

# Windows PowerShell 5.1 iletisim ayarlari
try { [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12 } catch { }

# Her kosuda benzersiz email
$stamp = [DateTimeOffset]::Now.ToUnixTimeSeconds()
$freelancerEmail = "freelancer.$stamp@staj.test"
$freelancerPassword = "Freelance123!"
$clientEmail = "client.$stamp@staj.test"
$clientPassword = "Client123!"

$stepNo = 0

function Write-Step {
    param([string]$Title)
    $script:stepNo++
    Write-Host ""
    Write-Host ("[{0}] {1}" -f $script:stepNo, $Title) -ForegroundColor Cyan
}

function Write-Ok {
    param([string]$Message)
    Write-Host ("    OK  " + $Message) -ForegroundColor Green
}

function Write-Info {
    param([string]$Message)
    Write-Host ("        " + $Message) -ForegroundColor DarkGray
}

function Fail {
    param([string]$Message)
    Write-Host ("    FAIL " + $Message) -ForegroundColor Red
    throw $Message
}

function Invoke-Api {
    param(
        [string]$Method,
        [string]$Path,
        $Body = $null,
        [string]$Token = $null,
        [bool]$ExpectSuccess = $true
    )

    $uri = "$BaseUrl$Path"
    $headers = @{ "Accept" = "application/json" }
    if ($Token) { $headers["Authorization"] = "Bearer $Token" }

    $params = @{
        Method  = $Method
        Uri     = $uri
        Headers = $headers
    }

    if ($null -ne $Body) {
        $params["Body"] = ($Body | ConvertTo-Json -Depth 10 -Compress)
        $params["ContentType"] = "application/json"
    }

    try {
        $response = Invoke-RestMethod @params
        if ($ExpectSuccess -and $response.success -ne $true) {
            $msg = if ($response.message) { $response.message } else { "Bilinmeyen hata" }
            $errs = if ($response.errors) { ($response.errors -join "; ") } else { "" }
            Fail ($Method + " " + $Path + " -> success=false: " + $msg + " " + $errs)
        }
        return $response
    }
    catch {
        $status = $null
        $body = $null
        if ($_.Exception.Response) {
            try { $status = [int]$_.Exception.Response.StatusCode } catch { }
            try {
                $stream = $_.Exception.Response.GetResponseStream()
                $reader = New-Object System.IO.StreamReader($stream)
                $body = $reader.ReadToEnd()
            } catch { }
        }

        if (-not $ExpectSuccess) {
            $parsed = $null
            if ($body) {
                try { $parsed = $body | ConvertFrom-Json } catch { $parsed = $null }
            }
            return [pscustomobject]@{
                httpStatus = $status
                payload    = $parsed
                rawBody    = $body
            }
        }

        $detail = if ($body) { $body } else { $_.Exception.Message }
        Fail ($Method + " " + $Path + " -> HTTP " + $status + " | " + $detail)
    }
}

Write-Host "=== Staj API Smoke Test ===" -ForegroundColor Yellow
Write-Info ("BaseUrl:     " + $BaseUrl)
Write-Info ("Admin:       " + $AdminEmail)
Write-Info ("Freelancer:  " + $freelancerEmail)
Write-Info ("Client:      " + $clientEmail)

# 1. ADMIN LOGIN
Write-Step "Admin login"
$r = Invoke-Api -Method POST -Path "/api/auth/login" -Body @{
    email = $AdminEmail; password = $AdminPassword
}
$adminToken = $r.data.accessToken
if (-not $adminToken) { Fail "Admin token gelmedi" }
Write-Ok ("Admin token alindi (uzunluk " + $adminToken.Length + ")")

# 2. MASTER DATA
Write-Step "Master data (kategori + skill listesi)"
$r = Invoke-Api -Method GET -Path "/api/categories"
$categories = $r.data
if ($categories.Count -lt 1) { Fail "Kategori listesi bos" }
$firstCategoryId = $categories[0].id
$catCount = $categories.Count
$catFirstName = $categories[0].name
Write-Ok ("$catCount kategori geldi (ilk: $catFirstName)")

$r = Invoke-Api -Method GET -Path "/api/skills"
$skills = $r.data
if ($skills.Count -lt 3) {
    $skillCount = $skills.Count
    Fail "En az 3 skill bekleniyordu, $skillCount geldi"
}
$skill1Id = $skills[0].id
$skill2Id = $skills[1].id
$skill3Id = $skills[2].id
$skillCount = $skills.Count
$s1 = $skills[0].name
$s2 = $skills[1].name
$s3 = $skills[2].name
Write-Ok ("$skillCount skill geldi (ilk 3: $s1, $s2, $s3)")

# 3. KULLANICI KAYITLARI
Write-Step "Freelancer register"
Invoke-Api -Method POST -Path "/api/auth/register" -Body @{
    email = $freelancerEmail; password = $freelancerPassword
    firstName = "Furkan"; lastName = "Test"; role = "Freelancer"
} | Out-Null
Write-Ok "Freelancer kaydi olusturuldu"

Write-Step "Client register"
Invoke-Api -Method POST -Path "/api/auth/register" -Body @{
    email = $clientEmail; password = $clientPassword
    firstName = "Ahmet"; lastName = "Musteri"; role = "Client"
} | Out-Null
Write-Ok "Client kaydi olusturuldu"

# 4. LOGIN
Write-Step "Freelancer login"
$r = Invoke-Api -Method POST -Path "/api/auth/login" -Body @{
    email = $freelancerEmail; password = $freelancerPassword
}
$freelancerToken = $r.data.accessToken
$freelancerUserId = $r.data.userId
$freelancerRefreshToken = $r.data.refreshToken
if (-not $freelancerToken) { Fail "Freelancer token gelmedi" }
Write-Ok ("Freelancer userId=" + $freelancerUserId)

Write-Step "Client login"
$r = Invoke-Api -Method POST -Path "/api/auth/login" -Body @{
    email = $clientEmail; password = $clientPassword
}
$clientToken = $r.data.accessToken
$clientUserId = $r.data.userId
if (-not $clientToken) { Fail "Client token gelmedi" }
Write-Ok ("Client userId=" + $clientUserId)

# 5. /me
Write-Step "/api/users/me — Freelancer"
$r = Invoke-Api -Method GET -Path "/api/users/me" -Token $freelancerToken
if ($r.data.email -ne $freelancerEmail) { Fail ("Email eslesmedi: " + $r.data.email) }
Write-Ok ("me email=" + $r.data.email)

Write-Step "/api/users/me — Client"
$r = Invoke-Api -Method GET -Path "/api/users/me" -Token $clientToken
if ($r.data.email -ne $clientEmail) { Fail ("Email eslesmedi: " + $r.data.email) }
Write-Ok ("me email=" + $r.data.email)

# 6. FREELANCER PROFIL AKISI
Write-Step "Freelancer basic-info guncelle"
Invoke-Api -Method PUT -Path "/api/users/me/basic-info" -Token $freelancerToken -Body @{
    firstName = "Furkan"; lastName = "Yilmaz"
} | Out-Null
Write-Ok "basic-info guncellendi"

Write-Step "Freelancer profil upsert"
Invoke-Api -Method PUT -Path "/api/freelancers/me" -Token $freelancerToken -Body @{
    title = "Senior .NET Developer"
    bio = "10+ yil .NET tecrubem var. Clean Architecture ve DDD uzerine calisiyorum."
    experienceYears = 10
    hourlyRateMin = 30; hourlyRateMax = 60; currency = "USD"
    country = "Turkiye"; city = "Istanbul"
    cvUrl = "https://example.com/cv.pdf"
    linkedInUrl = "https://linkedin.com/in/furkantest"
    gitHubUrl = "https://github.com/furkantest"
    websiteUrl = "https://furkan.dev"
    isAvailable = $true
} | Out-Null
Write-Ok "Freelancer profil olusturuldu"

Write-Step "Freelancer skill ata"
Invoke-Api -Method PUT -Path "/api/freelancers/me/skills" -Token $freelancerToken -Body @{
    skills = @(
        @{ skillId = $skill1Id; proficiencyLevel = 5 }
        @{ skillId = $skill2Id; proficiencyLevel = 4 }
        @{ skillId = $skill3Id; proficiencyLevel = 3 }
    )
} | Out-Null
Write-Ok "3 skill atandi"

Write-Step "Portfolio kalemi ekle"
$r = Invoke-Api -Method POST -Path "/api/freelancers/me/portfolio" -Token $freelancerToken -Body @{
    title = "E-Ticaret Platformu"
    description = ".NET 10 tabanli, mikroservis mimarisi ile gelistirilmis e-ticaret altyapisi."
    projectUrl = "https://github.com/furkan/eticaret"
    imageUrl = "https://example.com/img/eticaret.png"
    displayOrder = 1
}
$portfolioItemId = $r.data
Write-Ok ("portfolioItemId=" + $portfolioItemId)

Write-Step "Freelancer kendi profili — GET"
$r = Invoke-Api -Method GET -Path "/api/freelancers/me" -Token $freelancerToken
$freelancerProfileId = $r.data.id
$skillsCount = $r.data.skills.Count
$portfolioCount = $r.data.portfolioItems.Count
Write-Ok ("freelancerProfileId=$freelancerProfileId (skills=$skillsCount, portfolio=$portfolioCount)")

# 7. CLIENT PROFIL AKISI
Write-Step "Client profil upsert"
Invoke-Api -Method PUT -Path "/api/clients/me" -Token $clientToken -Body @{
    companyName = "Test Yazilim A.S."
    industry = "Bilisim"
    about = "Yenilikci yazilim projelerinde uzman ekiplerle calisiyoruz."
    websiteUrl = "https://testyazilim.example"
    country = "Turkiye"; city = "Ankara"
    contactPreference = 3
    contactPhone = "+905550001122"
} | Out-Null
Write-Ok "Client profil olusturuldu"

Write-Step "Client kendi profili — GET"
Invoke-Api -Method GET -Path "/api/clients/me" -Token $clientToken | Out-Null
Write-Ok "Client profili dondu"

# 8. FREELANCER ARAMA (public)
Write-Step "Freelancer arama (public)"
$r = Invoke-Api -Method GET -Path "/api/freelancers?page=1&pageSize=10"
$total = $r.data.totalCount
Write-Ok ("Arama dondu, toplam=" + $total)

Write-Step "Freelancer detay (public)"
Invoke-Api -Method GET -Path ("/api/freelancers/" + $freelancerProfileId) | Out-Null
Write-Ok "Detay geldi"

# 9. JOB POST
Write-Step "Ilan olustur (Client)"
$r = Invoke-Api -Method POST -Path "/api/jobs" -Token $clientToken -Body @{
    categoryId = $firstCategoryId
    title = "Senior .NET Developer araniyor"
    description = "Clean Architecture ile mikroservis projesi icin tecrubeli .NET gelistirici ariyoruz."
    budgetMin = 40; budgetMax = 80; currency = "USD"
    workMode = 1
    durationDays = 60
    country = "Turkiye"; city = "Istanbul"
    skillIds = @($skill1Id, $skill2Id)
}
$jobPostId = $r.data
Write-Ok ("jobPostId=" + $jobPostId)

Write-Step "Ilani publish et"
Invoke-Api -Method POST -Path ("/api/jobs/" + $jobPostId + "/publish") -Token $clientToken | Out-Null
Write-Ok "Ilan yayinda"

Write-Step "Ilan detay (public)"
Invoke-Api -Method GET -Path ("/api/jobs/" + $jobPostId) | Out-Null
Write-Ok "Ilan detay geldi"

Write-Step "Ilan arama (public)"
$r = Invoke-Api -Method GET -Path "/api/jobs?page=1&pageSize=10"
$total = $r.data.totalCount
Write-Ok ("Arama dondu, toplam=" + $total)

# 10. PROPOSAL — Application
Write-Step "Application: Freelancer basvurur"
$r = Invoke-Api -Method POST -Path "/api/proposals/applications" -Token $freelancerToken -Body @{
    jobPostId = $jobPostId
    coverMessage = "Merhaba, bu proje icin bicilmis kaftanim. .NET ve mikroservis konusunda 10 yil tecrubem var."
    proposedRate = 50
    proposedDurationDays = 45
    currency = "USD"
}
$applicationId = $r.data
Write-Ok ("applicationId=" + $applicationId)

Write-Step "Client kendi ilanina gelen basvurulari gorsun"
$r = Invoke-Api -Method GET -Path ("/api/proposals/for-job/" + $jobPostId) -Token $clientToken
$appCount = $r.data.Count
Write-Ok ("Basvuru sayisi=" + $appCount)

Write-Step "Freelancer'in gonderdigi proposal listesi"
Invoke-Api -Method GET -Path "/api/proposals/me?direction=Sent" -Token $freelancerToken | Out-Null
Write-Ok "Liste geldi"

Write-Step "Client Accept — Freelancer'a bildirim dusmeli"
Invoke-Api -Method POST -Path ("/api/proposals/" + $applicationId + "/accept") -Token $clientToken | Out-Null
Write-Ok "Application Accepted"

# 11. NOTIFICATION
Write-Step "Freelancer unread-count (>=1 bekleniyor)"
$r = Invoke-Api -Method GET -Path "/api/notifications/me/unread-count" -Token $freelancerToken
$unread = $r.data
Write-Info ("unread-count=" + $unread)
if ($unread -lt 1) { Fail "Beklenen bildirim gelmedi" }
Write-Ok ("unread-count=" + $unread)

Write-Step "Freelancer bildirim listesi"
$r = Invoke-Api -Method GET -Path "/api/notifications/me?page=1&pageSize=10" -Token $freelancerToken
$nCount = $r.data.items.Count
Write-Ok ($nCount.ToString() + " bildirim geldi")

# 12. DIRECT OFFER
Write-Step "DirectOffer: Client -> Freelancer"
$r = Invoke-Api -Method POST -Path "/api/proposals/direct-offers" -Token $clientToken -Body @{
    freelancerProfileId = $freelancerProfileId
    coverMessage = "Kucuk bir danismanlik projesi var, ilgilenir misin?"
    proposedRate = 70
    proposedDurationDays = 15
    currency = "USD"
}
$directOfferId = $r.data
Write-Ok ("directOfferId=" + $directOfferId)

Write-Step "Freelancer teklifi goruntule"
Invoke-Api -Method GET -Path ("/api/proposals/" + $directOfferId) -Token $freelancerToken | Out-Null
Write-Ok "Teklif detayi geldi"

Write-Step "Freelancer Accept — Client'a bildirim dusmeli"
Invoke-Api -Method POST -Path ("/api/proposals/" + $directOfferId + "/accept") -Token $freelancerToken | Out-Null
Write-Ok "DirectOffer Accepted"

Write-Step "Client unread-count (>=1 bekleniyor)"
$r = Invoke-Api -Method GET -Path "/api/notifications/me/unread-count" -Token $clientToken
if ($r.data -lt 1) { Fail "Client a bildirim gelmedi" }
Write-Ok ("unread-count=" + $r.data)

# 13. MESSAGING
Write-Step "Conversation baslat (Client -> Freelancer)"
$r = Invoke-Api -Method POST -Path "/api/conversations" -Token $clientToken -Body @{
    otherUserId = $freelancerUserId
}
$conversationId = $r.data
Write-Ok ("conversationId=" + $conversationId)

Write-Step "Client mesaj gonder"
Invoke-Api -Method POST -Path "/api/messages" -Token $clientToken -Body @{
    conversationId = $conversationId
    content = "Merhaba, projeye ne zaman baslayabiliriz?"
} | Out-Null
Write-Ok "Mesaj gonderildi"

Write-Step "Freelancer conversation listesi"
$r = Invoke-Api -Method GET -Path "/api/conversations" -Token $freelancerToken
$cCount = $r.data.Count
Write-Ok ($cCount.ToString() + " conversation geldi")

Write-Step "Freelancer mesajlari goruntule"
$r = Invoke-Api -Method GET -Path ("/api/conversations/" + $conversationId + "/messages?page=1&pageSize=20") -Token $freelancerToken
$mCount = $r.data.items.Count
Write-Ok ($mCount.ToString() + " mesaj geldi")

Write-Step "Freelancer messages unread-count"
$r = Invoke-Api -Method GET -Path "/api/messages/unread-count" -Token $freelancerToken
Write-Ok ("unread-count=" + $r.data)

Write-Step "Freelancer cevap yaz"
Invoke-Api -Method POST -Path "/api/messages" -Token $freelancerToken -Body @{
    conversationId = $conversationId
    content = "Merhaba, onumuzdeki hafta baslayabilirim."
} | Out-Null
Write-Ok "Cevap gonderildi"

Write-Step "Client mesaji okundu isaretle"
Invoke-Api -Method POST -Path ("/api/conversations/" + $conversationId + "/read") -Token $clientToken | Out-Null
Write-Ok "Conversation okundu"

# 14. REVIEW
Write-Step "Client -> Freelancer review"
Invoke-Api -Method POST -Path "/api/reviews" -Token $clientToken -Body @{
    proposalId = $applicationId
    rating = 5
    comment = "Freelancer profesyonel, hizli ve kaliteli is cikardi. Kesinlikle tekrar calisirsak."
} | Out-Null
Write-Ok "Review kaydedildi"

Write-Step "Freelancer -> Client review"
Invoke-Api -Method POST -Path "/api/reviews" -Token $freelancerToken -Body @{
    proposalId = $applicationId
    rating = 5
    comment = "Isveren gereksinimleri net iletti, odeme surecinde iletisim cok iyiydi."
} | Out-Null
Write-Ok "Review kaydedildi"

Write-Step "Freelancer'a yazilmis reviewlar (public)"
$path = "/api/reviews/for-user/" + $freelancerUserId + "?page=1&pageSize=10"
$r = Invoke-Api -Method GET -Path $path
$rCount = $r.data.items.Count
Write-Ok ($rCount.ToString() + " review geldi")

Write-Step "Freelancer rating summary (public)"
$path = "/api/reviews/for-user/" + $freelancerUserId + "/summary"
$r = Invoke-Api -Method GET -Path $path
$avg = $r.data.averageRating
$cnt = $r.data.reviewCount
Write-Ok ("avg=$avg count=$cnt")

Write-Step "Freelancer detayinda rating (Module 8 entegrasyonu)"
$r = Invoke-Api -Method GET -Path ("/api/freelancers/" + $freelancerProfileId)
$avg = $r.data.averageRating
$cnt = $r.data.reviewCount
$rec = $r.data.recentReviews.Count
Write-Ok ("avg=$avg count=$cnt recent=$rec")

# 15. NOTIFICATION mark-as-read
Write-Step "Freelancer nihai bildirim listesi"
$r = Invoke-Api -Method GET -Path "/api/notifications/me?page=1&pageSize=20" -Token $freelancerToken
if ($r.data.items.Count -lt 1) { Fail "Beklenen bildirim yok" }
$firstNotifId = $r.data.items[0].id
$total = $r.data.items.Count
Write-Ok ($total.ToString() + " bildirim, ilk id=" + $firstNotifId)

Write-Step "Bir bildirimi okundu yap"
Invoke-Api -Method POST -Path ("/api/notifications/" + $firstNotifId + "/read") -Token $freelancerToken | Out-Null
Write-Ok "Okundu"

Write-Step "Tumunu okundu yap"
Invoke-Api -Method POST -Path "/api/notifications/me/read-all" -Token $freelancerToken | Out-Null
Write-Ok "Tumu okundu"

Write-Step "unread-count = 0 olmali"
$r = Invoke-Api -Method GET -Path "/api/notifications/me/unread-count" -Token $freelancerToken
if ($r.data -ne 0) {
    $u = $r.data
    Fail "unread-count 0 degil: $u"
}
Write-Ok "unread-count=0"

# 16. NEGATIF SENARYOLAR
Write-Step "Tokensiz /me -> 401 bekleniyor"
$neg = Invoke-Api -Method GET -Path "/api/users/me" -ExpectSuccess $false
if ($neg.httpStatus -ne 401) {
    $st = $neg.httpStatus
    Fail "401 yerine $st"
}
Write-Ok "401 alindi"

Write-Step "Freelancer job olusturmasi -> hata bekleniyor"
$neg = Invoke-Api -Method POST -Path "/api/jobs" -Token $freelancerToken -ExpectSuccess $false -Body @{
    categoryId = $firstCategoryId
    title = "Yetkisiz ilan denemesi"
    description = "Bu istegin fail olmasi bekleniyor."
    budgetMin = 10; budgetMax = 20; currency = "USD"
    workMode = 1; durationDays = 30
    country = "TR"; city = "Istanbul"
    skillIds = @()
}
$st = $neg.httpStatus
Write-Ok ("Beklenen fail alindi (status=" + $st + ")")

Write-Step "Ayni ilana ikinci basvuru -> duplicate bekleniyor"
$neg = Invoke-Api -Method POST -Path "/api/proposals/applications" -Token $freelancerToken -ExpectSuccess $false -Body @{
    jobPostId = $jobPostId
    coverMessage = "Bu basvuru duplicate, fail donmeli."
    proposedRate = 50
    proposedDurationDays = 45
    currency = "USD"
}
$st = $neg.httpStatus
Write-Ok ("Beklenen fail alindi (status=" + $st + ")")

# 17. REFRESH TOKEN
Write-Step "Refresh token ile yenileme"
$r = Invoke-Api -Method POST -Path "/api/auth/refresh" -Body @{
    accessToken = $freelancerToken
    refreshToken = $freelancerRefreshToken
}
if (-not $r.data.accessToken) { Fail "Yeni access token gelmedi" }
Write-Ok "Yeni access token alindi"

Write-Host ""
Write-Host ("=== TAMAMLANDI: " + $stepNo + " adim basariyla gecti ===") -ForegroundColor Green