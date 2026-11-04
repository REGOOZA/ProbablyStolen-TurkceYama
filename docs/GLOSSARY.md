# Probably Stolen — Turkish translation guide

Game: a pawn-shop / trading sim on a dystopian space station. The player runs a small store in
the poor **Lower Levels**; the rich elites live in the **Upper Levels**. Clients bring (often
stolen) items, you appraise, haggle, pay rent, deal with Security, factions, contraband, etc.
Tone: gritty, sardonic, lots of humour and character voice in dialogue.

## Style rules

- Natural, fluent Turkish as a professional game localizer would write it. Not word-for-word.
- Address the player with **"sen"** (informal). Characters keep their own voice: a pompous
  character stays pompous, an official may use "siz" if it fits, slang stays slangy.
- UI labels/buttons: short, imperative or noun form ("Günü Bitir", "Seçenekler", "Ana Menü").
- Item `_name`: Title Case style like the source ("Hurda Metal Yığını"). `_desc`/`_flavor`: full sentences.
- Keep brand / invented proper names as-is (see list). Keep people's names as-is.
- Use correct Turkish characters: ç ğ ı İ ö ş ü (and â where common, e.g. "dükkân", "kâr").
- Turkish numbers: keep the source digits/format; do not change `{0}` formats.

## Hard technical rules (the game will break otherwise)

1. Copy every placeholder **exactly**: `{0}`, `{1}`, `{count}`, `{0:P0}` … Same count, same braces.
   Word order may change around them.
2. Copy rich-text tags exactly and keep them balanced: `<b>…</b>`, `<i>…</i>`, `<u>…</u>`,
   `<color=#CC…>…</color>`, `<wave a=…>…</wave>`, `<slide a=…>…</slide>`, `<sprite name="c" tint=1 >`.
   Translate the text *inside* the tags.
3. Keep line breaks (`\n` in JSON) in the same places/number.
4. Keep `*emphasis*` asterisks around the equivalent Turkish word.
5. Avoid attaching Turkish suffixes directly to placeholders (vowel harmony is unknown):
   prefer "{0} Kredi", "Fiyat: {0}", "{0} adlı müşteri", or rephrase.
6. Never translate or change keys. Output must contain **every key** of your input.
7. If a string is just a code / symbol / number / "placeholder", copy it unchanged.

## Glossary (use consistently)

| English | Türkçe |
|---|---|
| Lower Levels / Lower Level (adj.) | Alt Katlar / Alt Kat |
| Upper Levels / Upper Level (adj.) | Üst Katlar / Üst Kat |
| Station / Station Command | İstasyon / İstasyon Komutanlığı |
| Security / Station Security / Security Department | Güvenlik / İstasyon Güvenliği / Güvenlik Departmanı |
| Security Officer(s) / Officer Lun | Güvenlik Görevlisi (-leri) / Memur Lun |
| Store (the player's shop) | Dükkân |
| Shopkeeper | Dükkâncı |
| Credits (money) / CREDITS | Kredi / KREDİ |
| Credits (MainMenu "credit" = game credits screen) | Emeği Geçenler |
| Reputation | İtibar |
| Contraband | Kaçak Mal |
| Black Market | Karaborsa |
| Underground Exchange | Yeraltı Borsası |
| Cartel / Republic / Revolution / First Revolution | Kartel / Cumhuriyet / Devrim / Birinci Devrim |
| Landlord / Rent | Ev Sahibi / Kira |
| The Mentor | Akıl Hocası |
| Scavenger | Hurdacı |
| Display Case | Vitrin |
| Storage Bay / Bay | Depo Bölmesi / Bölme |
| Negotiation Counter / Negotiation Interface | Pazarlık Tezgâhı / Pazarlık Arayüzü |
| Appraiser / Appraiser Board | Değerleyici / Değerleme Panosu |
| Marketplace Request(s) | Pazar Talebi (Talepleri) |
| Perk(s) | Avantaj(lar) |
| Network Upgrade | Ağ Yükseltmesi |
| Base Value / Value | Taban Değer / Değer |
| Markup / Margin / Discount | Kâr Payı / Marj / İndirim |
| Module(s) / Chip | Modül(ler) / Çip |
| Power Source / Power Consumption | Güç Kaynağı / Güç Tüketimi |
| Energy Cell(s) | Enerji Hücresi (Hücreleri) |
| Keycard / Blank Keycard | Anahtar Kart / Boş Anahtar Kart |
| Cryptographic Desequencer (Chip) | Kriptografik Dizi Çözücü (Çipi) |
| Command / Security (keycard permission) | Komuta / Güvenlik |
| Scrap Metal / Metal Ore / Electronic Parts / Screws | Hurda Metal / Metal Cevheri / Elektronik Parçalar / Vidalar |
| Materials / Minerals / Supplies | Malzemeler / Mineraller / Tedarik |
| Food / Water / Pure Water / High-Quality Water | Yiyecek / Su / Saf Su / Yüksek Kaliteli Su |
| Household Goods / Luxury Items / Medical Supplies | Ev Eşyaları / Lüks Eşyalar / Tıbbi Malzemeler |
| Food Stamps | Gıda Kuponu |
| Dumping Grounds / Dumping Ground | Çöplük |
| Abandoned Sections | Terk Edilmiş Bölümler |
| Commissary | Kantin |
| Raid / Inspection / Permit | Baskın / Denetim / İzin Belgesi |
| Purity / Grade / Condition | Saflık / Kalite / Durum |
| Addiction | Bağımlılık |
| Dream Dust / Dream Cap / Dream Cap Mush | Rüya Tozu / Rüya Mantarı / Rüya Mantarı Lapası |
| Liquid Luck | Sıvı Şans |
| Lower Levels Lottery | Alt Katlar Piyangosu |
| Shareholder / Citizen | Hissedar / Vatandaş |
| Day / End the Day | Gün / Günü Bitir |
| Steam Workshop / Workshop (in-game) | Steam Atölyesi / Atölye |
| Hitman | Tetikçi |
| Grandpa Cheng | Cheng Dede |
| Trading Manual / Handbook | Ticaret El Kitabı / El Kitabı |
| Magnifying Glass | Büyüteç |
| Chemical Scanner / Scanner | Kimyasal Tarayıcı / Tarayıcı |
| Furnace / Evaporator / Heating Plate / Chem Finisher | Fırın / Buharlaştırıcı / Isıtma Plakası / Kimyasal Son İşlemci |

**Keep unchanged (brands / invented names / people):** Nutrifruit, Ribwich, Basewater, Ghostwater,
Rustwater, Gutterflow, Oxymore, Immunivax, Phanta, PEAT, Hemingway, PAX, ETERNAL, Hypertech,
Red Imp Brewery, GP Mining Coop, First Bank, Sentinel, Kotton, Georgio, Serpico, Lou, Nellis,
Eleanor, Ashish, Odin Haraldsson, Joe Wild, Discord, Steam, SMG, DNA, AI, RAS, RDS.
Plural/possessive of these follow Turkish rules with an apostrophe: "Georgio'nun", "Nutrifruit'ler".
Role-only names in ClientName are translated (Farmer → Çiftçi, Foreman → Ustabaşı, Food Buyer → Yiyecek Alıcısı).

If you meet another recurring term not listed, pick one good translation and use it consistently.
