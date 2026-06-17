// CombatSimulationController.cs
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tugas 5: Simulasi Menembak & Reload + Damage & Heal.
/// Menggabungkan sistem Ammo/Reload (Image fillAmount) dan HP/Damage/Heal (Color + Alpha overlay).
/// Versi polish: HP rendah membuat layar berdenyut merah, dan status text balik ke "Siap" otomatis.
/// </summary>
public class CombatSimulationController : MonoBehaviour
{
    [Header("Text UI")]
    [SerializeField] private TextMeshProUGUI _ammoText;
    [SerializeField] private TextMeshProUGUI _hpText;
    [SerializeField] private TextMeshProUGUI _statusText;

    [Header("Buttons")]
    [SerializeField] private Button _shootButton;
    [SerializeField] private Button _reloadButton;
    [SerializeField] private Button _damageButton;
    [SerializeField] private Button _healButton;

    [Header("Images")]
    [SerializeField] private Image _reloadFillImage;   // Image Type = Filled, Fill Method = Horizontal
    [SerializeField] private Image _effectOverlay;     // Image putih full screen, Alpha 0, Raycast Target OFF

    [Header("Ammo Settings")]
    [SerializeField] private int _maxAmmo = 15;
    [SerializeField] private float _reloadDuration = 2f;

    [Header("HP Settings")]
    [SerializeField] private int _maxHP = 100;
    [SerializeField] private int _damageAmount = 10;
    [SerializeField] private int _healAmount = 10;

    [Header("Effect Settings")]
    [SerializeField] private float _effectFadeSpeed = 1.5f; // seberapa cepat alpha turun per detik

    [Header("Low HP Pulse (Breathing)")]
    [SerializeField] private float _lowHpPulseMin = 0.12f;  // alpha paling redup saat berdenyut
    [SerializeField] private float _lowHpPulseMax = 0.30f;  // alpha paling pekat saat berdenyut
    [SerializeField] private float _lowHpPulseSpeed = 2.5f; // kecepatan denyut

    [Header("Status Text")]
    [SerializeField] private float _statusResetDelay = 1.5f; // status balik ke "Siap" setelah sekian detik

    [Header("Status Pop Animation")]
    [SerializeField] private float _statusPopScale = 1.4f;    // sebesar apa teks "meletup" saat muncul
    [SerializeField] private float _statusPopSpeed = 6f;      // seberapa cepat balik ke ukuran normal

    // --- State Ammo ---
    private int _currentAmmo;
    private bool _isReloading;
    private float _reloadTimer;

    // --- State HP ---
    private int _currentHP;

    // --- State Overlay ---
    private Color _overlayColor; // warna overlay yang sedang ditampilkan (RGBA)

    // --- State Status ---
    private float _statusTimer;  // hitung mundur sebelum status balik ke "Siap"
    private RectTransform _statusRect; // dipakai untuk animasi pop (scale)

    private void Start()
    {
        _currentAmmo = _maxAmmo;
        _currentHP = _maxHP;

        // Setup Reload Bar lewat script (jaga-jaga kalau lupa diatur di Inspector)
        _reloadFillImage.type = Image.Type.Filled;
        _reloadFillImage.fillMethod = Image.FillMethod.Horizontal;
        _reloadFillImage.fillAmount = 0f;

        // Overlay mulai transparan total
        _overlayColor = new Color(1f, 0f, 0f, 0f);
        _effectOverlay.color = _overlayColor;

        // Pasang listener tombol lewat script (cara AddListener)
        _shootButton.onClick.AddListener(Shoot);
        _reloadButton.onClick.AddListener(StartReload);
        _damageButton.onClick.AddListener(TakeDamage);
        _healButton.onClick.AddListener(Heal);

        UpdateAmmoText();
        UpdateHpText();
        UpdateButtons();
        _statusText.text = "Siap";
        _statusRect = _statusText.rectTransform; // simpan untuk animasi pop
    }

    private void Update()
    {
        // Proses reload berjalan tiap frame
        if (_isReloading)
        {
            _reloadTimer += Time.deltaTime;

            // Ubah waktu reload jadi nilai fillAmount (0 sampai 1)
            float reloadProgress = _reloadTimer / _reloadDuration;
            _reloadFillImage.fillAmount = reloadProgress;

            if (_reloadTimer >= _reloadDuration)
            {
                FinishReload();
            }
        }

        // Efek overlay (memudar / berdenyut) tiap frame
        FadeEffectOverlay();

        // Status text balik ke "Siap" otomatis
        UpdateStatusTimer();

        // Animasi pop status text (mengecil balik ke normal tiap frame)
        UpdateStatusPop();
    }

    // ---------------- SHOOT / RELOAD ----------------
    private void Shoot()
    {
        if (_isReloading) return;

        if (_currentAmmo <= 0)
        {
            SetStatus("Ammo habis! Tekan Reload");
            return;
        }

        _currentAmmo--;
        SetStatus("Menembak");
        UpdateAmmoText();
        UpdateButtons();
    }

    private void StartReload()
    {
        if (_isReloading) return;
        if (_currentAmmo >= _maxAmmo) return; // tidak perlu reload kalau sudah penuh

        _isReloading = true;
        _reloadTimer = 0f;
        _reloadFillImage.fillAmount = 0f;
        SetStatus("Reloading...");
        UpdateButtons(); // shoot & reload mati selama reload
    }

    private void FinishReload()
    {
        _isReloading = false;
        _currentAmmo = _maxAmmo;
        _reloadFillImage.fillAmount = 0f; // bar langsung kosong setelah reload selesai
        SetStatus("Reload selesai");
        UpdateAmmoText();
        UpdateButtons();
    }

    // ---------------- DAMAGE / HEAL ----------------
    private void TakeDamage()
    {
        _currentHP -= _damageAmount;
        _currentHP = Mathf.Clamp(_currentHP, 0, _maxHP);
        SetStatus("Player terkena damage");

        // Efek merah transparan
        TriggerEffect(new Color(1f, 0f, 0f, 0.6f));

        UpdateHpText();
        UpdateButtons();
    }

    private void Heal()
    {
        _currentHP += _healAmount;
        _currentHP = Mathf.Clamp(_currentHP, 0, _maxHP);
        SetStatus("Player mendapatkan heal");

        // Efek hijau transparan
        TriggerEffect(new Color(0f, 1f, 0f, 0.4f));

        UpdateHpText();
        UpdateButtons();
    }

    // ---------------- EFEK OVERLAY ----------------
    private void TriggerEffect(Color color)
    {
        _overlayColor = color;
        _effectOverlay.color = _overlayColor;
    }

    private void FadeEffectOverlay()
    {
        if (_currentHP <= 25)
        {
            // HP rendah: layar berdenyut merah (efek "breathing" seperti game horror).
            // Sinus menghasilkan nilai naik-turun halus antara min dan max.
            float t = (Mathf.Sin(Time.time * _lowHpPulseSpeed) + 1f) * 0.5f; // 0..1
            float pulseAlpha = Mathf.Lerp(_lowHpPulseMin, _lowHpPulseMax, t);

            // Kalau alpha sudah masuk zona denyut, pastikan warnanya merah
            if (_overlayColor.a <= _lowHpPulseMax)
            {
                _overlayColor = new Color(1f, 0f, 0f, _overlayColor.a);
            }

            if (_overlayColor.a > pulseAlpha)
            {
                // Masih turun dari efek damage/heal yang barusan → fade dulu
                _overlayColor.a = Mathf.MoveTowards(_overlayColor.a, pulseAlpha, _effectFadeSpeed * Time.deltaTime);
            }
            else
            {
                // Sudah di zona rendah → ikuti denyut
                _overlayColor.a = pulseAlpha;
            }
        }
        else
        {
            // HP normal: alpha turun perlahan sampai benar-benar transparan
            _overlayColor.a = Mathf.MoveTowards(_overlayColor.a, 0f, _effectFadeSpeed * Time.deltaTime);
        }

        _effectOverlay.color = _overlayColor;
    }

    // ---------------- STATUS TEXT ----------------
    private void SetStatus(string message)
    {
        _statusText.text = message;
        _statusTimer = _statusResetDelay; // mulai hitung mundur balik ke "Siap"

        // Mulai efek "pop": langsung besarkan, nanti mengecil pelan di UpdateStatusPop()
        if (_statusRect != null)
        {
            _statusRect.localScale = Vector3.one * _statusPopScale;
        }
    }

    private void UpdateStatusPop()
    {
        if (_statusRect == null) return;

        // Kembalikan ukuran ke normal (1,1,1) secara halus tiap frame
        _statusRect.localScale = Vector3.Lerp(
            _statusRect.localScale,
            Vector3.one,
            _statusPopSpeed * Time.deltaTime);
    }

    private void UpdateStatusTimer()
    {
        if (_statusTimer <= 0f) return;

        _statusTimer -= Time.deltaTime;

        // Saat waktu habis, balik ke "Siap" (kecuali sedang reload, biar tetap "Reloading...")
        if (_statusTimer <= 0f && !_isReloading)
        {
            _statusText.text = "Siap";
        }
    }

    // ---------------- UPDATE TEXT (RICH TEXT WARNA) ----------------
    private void UpdateAmmoText()
    {
        string color;
        if (_currentAmmo <= 0)
            color = "red";
        else if (_currentAmmo <= 5)
            color = "yellow";
        else
            color = "white";

        _ammoText.text = $"<color={color}>{_currentAmmo}/{_maxAmmo}</color>";
    }

    private void UpdateHpText()
    {
        string color;
        if (_currentHP <= 25)
            color = "red";
        else if (_currentHP <= 50)
            color = "yellow";
        else
            color = "white";

        _hpText.text = $"<color={color}>HP {_currentHP}/{_maxHP}</color>";
    }

    // ---------------- INTERACTABLE TOMBOL ----------------
    private void UpdateButtons()
    {
        // Saat reload: Shoot & Reload mati
        _shootButton.interactable = !_isReloading && _currentAmmo > 0;
        _reloadButton.interactable = !_isReloading && _currentAmmo < _maxAmmo;

        // Heal mati kalau HP penuh, Damage mati kalau HP 0
        _healButton.interactable = _currentHP < _maxHP;
        _damageButton.interactable = _currentHP > 0;
    }
}