export enum Currency {
    // ???? «бЏгб«  «бЏ—»н…
    EGP = 1,    // ћдне г’—н
    SAR = 2,    // —н«б ”Џжѕн
    AED = 3,    // ѕ—ег ≈г«—« н
    KWD = 4,    // ѕнд«— яжн н
    QAR = 5,    // —н«б ёЎ—н
    BHD = 9,    // ѕнд«— »Ќ—ндн
    OMR = 10,   // —н«б Џг«дн
    JOD = 11,   // ѕнд«— √—ѕдн
    LBP = 12,   // бн—… б»д«дн…
    SYP = 13,   // бн—… ”ж—н…
    IQD = 14,   // ѕнд«— Џ—«ён
    YER = 15,   // —н«б нгдн
    LYD = 16,   // ѕнд«— бн»н
    TND = 17,   // ѕнд«—  жд”н
    DZD = 18,   // ѕнд«— ћ“«∆—н
    MAD = 19,   // ѕ—ег гџ—»н
    SDG = 20,   // ћдне ”жѕ«дн

    // ?? «бЏгб«  «б—∆н”н…
    USD = 6,    // ѕжб«— √г—нян
    EUR = 7,    // нж—ж
    GBP = 8,    // ћдне ≈” —бндн
    CAD = 21,   // ѕжб«— ядѕн
    AUD = 22,   // ѕжб«— √” —«бн
    CHF = 23,   // Ё—дя ”жн”—н
    JPY = 24,   // нд н«»«дн
    CNY = 25,   // нж«д ’ндн

    // ?? Џгб«  ¬”нжн…
    INR = 26,   // —ж»н… едѕн…
    PKR = 27,   // —ж»н… »«я” «дн…
    BDT = 28,   //  «я« »дџб«ѕн‘н…
    MYR = 29,   // —ндџн  г«бн“н
    IDR = 30,   // —ж»н… ≈дѕждн”н…
    TRY = 31,   // бн—…  —ян…
    IRR = 32,   // —н«б ≈н—«дн
    ILS = 33,   // ‘няб ≈”—«∆нбн

    // ?? Џгб«  √Ё—нён…
    ZAR = 34,   // —«дѕ ћдж» √Ё—нён
    NGN = 35,   // д«н—« днћн—н
    KES = 36,   // ‘бд яндн

    // ?? Џгб«  √ќ—м
    NZD = 37,   // ѕжб«— днж“нбдѕн
    SGD = 38,   // ѕжб«— ”дџ«Ёж—н
    HKD = 39,   // ѕжб«— еждџ яждџ
    KRW = 40,   // жжд яж—н
    BRL = 41,   // —н«б »—«“нбн
    MXN = 42,   // »н“ж гя”нян
    RUB = 43,   // —ж»б —ж”н
    SEK = 44,   // я—жд… ”жнѕн…
    NOK = 45,   // я—жд… д—жнћн…
    DKK = 46,   // я—жд… ѕдг«—ян…
    PLN = 47,   // “бж н »жбдѕн
    THB = 48,   // »«   «нбдѕн
    PHP = 49,   // »н“ж Ёб»ндн
    VND = 50    // ѕждџ Ён д«гн
}

export const CurrencyLabels: { [key in Currency]: string } = {
    // ???? «бЏгб«  «бЏ—»н…
    [Currency.EGP]: 'ћдне г’—н',
    [Currency.SAR]: '—н«б ”Џжѕн',
    [Currency.AED]: 'ѕ—ег ≈г«—« н',
    [Currency.KWD]: 'ѕнд«— яжн н',
    [Currency.QAR]: '—н«б ёЎ—н',
    [Currency.BHD]: 'ѕнд«— »Ќ—ндн',
    [Currency.OMR]: '—н«б Џг«дн',
    [Currency.JOD]: 'ѕнд«— √—ѕдн',
    [Currency.LBP]: 'бн—… б»д«дн…',
    [Currency.SYP]: 'бн—… ”ж—н…',
    [Currency.IQD]: 'ѕнд«— Џ—«ён',
    [Currency.YER]: '—н«б нгдн',
    [Currency.LYD]: 'ѕнд«— бн»н',
    [Currency.TND]: 'ѕнд«—  жд”н',
    [Currency.DZD]: 'ѕнд«— ћ“«∆—н',
    [Currency.MAD]: 'ѕ—ег гџ—»н',
    [Currency.SDG]: 'ћдне ”жѕ«дн',

    // ?? «бЏгб«  «б—∆н”н…
    [Currency.USD]: 'ѕжб«— √г—нян',
    [Currency.EUR]: 'нж—ж',
    [Currency.GBP]: 'ћдне ≈” —бндн',
    [Currency.CAD]: 'ѕжб«— ядѕн',
    [Currency.AUD]: 'ѕжб«— √” —«бн',
    [Currency.CHF]: 'Ё—дя ”жн”—н',
    [Currency.JPY]: 'нд н«»«дн',
    [Currency.CNY]: 'нж«д ’ндн',

    // ?? Џгб«  ¬”нжн…
    [Currency.INR]: '—ж»н… едѕн…',
    [Currency.PKR]: '—ж»н… »«я” «дн…',
    [Currency.BDT]: ' «я« »дџб«ѕн‘н…',
    [Currency.MYR]: '—ндџн  г«бн“н',
    [Currency.IDR]: '—ж»н… ≈дѕждн”н…',
    [Currency.TRY]: 'бн—…  —ян…',
    [Currency.IRR]: '—н«б ≈н—«дн',
    [Currency.ILS]: '‘няб ≈”—«∆нбн',

    // ?? Џгб«  √Ё—нён…
    [Currency.ZAR]: '—«дѕ ћдж» √Ё—нён',
    [Currency.NGN]: 'д«н—« днћн—н',
    [Currency.KES]: '‘бд яндн',

    // ?? Џгб«  √ќ—м
    [Currency.NZD]: 'ѕжб«— днж“нбдѕн',
    [Currency.SGD]: 'ѕжб«— ”дџ«Ёж—н',
    [Currency.HKD]: 'ѕжб«— еждџ яждџ',
    [Currency.KRW]: 'жжд яж—н',
    [Currency.BRL]: '—н«б »—«“нбн',
    [Currency.MXN]: '»н“ж гя”нян',
    [Currency.RUB]: '—ж»б —ж”н',
    [Currency.SEK]: 'я—жд… ”жнѕн…',
    [Currency.NOK]: 'я—жд… д—жнћн…',
    [Currency.DKK]: 'я—жд… ѕдг«—ян…',
    [Currency.PLN]: '“бж н »жбдѕн',
    [Currency.THB]: '»«   «нбдѕн',
    [Currency.PHP]: '»н“ж Ёб»ндн',
    [Currency.VND]: 'ѕждџ Ён д«гн',
};

export const CurrencyOptions = [
    // ???? «бЏгб«  «бЏ—»н…
    { label: '???? ћдне г’—н (EGP)', value: Currency.EGP },
    { label: '???? —н«б ”Џжѕн (SAR)', value: Currency.SAR },
    { label: '???? ѕ—ег ≈г«—« н (AED)', value: Currency.AED },
    { label: '???? ѕнд«— яжн н (KWD)', value: Currency.KWD },
    { label: '???? —н«б ёЎ—н (QAR)', value: Currency.QAR },
    { label: '???? ѕнд«— »Ќ—ндн (BHD)', value: Currency.BHD },
    { label: '???? —н«б Џг«дн (OMR)', value: Currency.OMR },
    { label: '???? ѕнд«— √—ѕдн (JOD)', value: Currency.JOD },
    { label: '???? бн—… б»д«дн… (LBP)', value: Currency.LBP },
    { label: '???? бн—… ”ж—н… (SYP)', value: Currency.SYP },
    { label: '???? ѕнд«— Џ—«ён (IQD)', value: Currency.IQD },
    { label: '???? —н«б нгдн (YER)', value: Currency.YER },
    { label: '???? ѕнд«— бн»н (LYD)', value: Currency.LYD },
    { label: '???? ѕнд«—  жд”н (TND)', value: Currency.TND },
    { label: '???? ѕнд«— ћ“«∆—н (DZD)', value: Currency.DZD },
    { label: '???? ѕ—ег гџ—»н (MAD)', value: Currency.MAD },
    { label: '???? ћдне ”жѕ«дн (SDG)', value: Currency.SDG },

    // ?? «бЏгб«  «б—∆н”н…
    { label: '???? ѕжб«— √г—нян (USD)', value: Currency.USD },
    { label: '???? нж—ж (EUR)', value: Currency.EUR },
    { label: '???? ћдне ≈” —бндн (GBP)', value: Currency.GBP },
    { label: '???? ѕжб«— ядѕн (CAD)', value: Currency.CAD },
    { label: '???? ѕжб«— √” —«бн (AUD)', value: Currency.AUD },
    { label: '???? Ё—дя ”жн”—н (CHF)', value: Currency.CHF },
    { label: '???? нд н«»«дн (JPY)', value: Currency.JPY },
    { label: '???? нж«д ’ндн (CNY)', value: Currency.CNY },

    // ?? Џгб«  ¬”нжн…
    { label: '???? —ж»н… едѕн… (INR)', value: Currency.INR },
    { label: '???? —ж»н… »«я” «дн… (PKR)', value: Currency.PKR },
    { label: '????  «я« »дџб«ѕн‘н… (BDT)', value: Currency.BDT },
    { label: '???? —ндџн  г«бн“н (MYR)', value: Currency.MYR },
    { label: '???? —ж»н… ≈дѕждн”н… (IDR)', value: Currency.IDR },
    { label: '???? бн—…  —ян… (TRY)', value: Currency.TRY },
    { label: '???? —н«б ≈н—«дн (IRR)', value: Currency.IRR },
    { label: '???? ‘няб ≈”—«∆нбн (ILS)', value: Currency.ILS },

    // ?? Џгб«  √Ё—нён…
    { label: '???? —«дѕ ћдж» √Ё—нён (ZAR)', value: Currency.ZAR },
    { label: '???? д«н—« днћн—н (NGN)', value: Currency.NGN },
    { label: '???? ‘бд яндн (KES)', value: Currency.KES },

    // ?? Џгб«  √ќ—м
    { label: '???? ѕжб«— днж“нбдѕн (NZD)', value: Currency.NZD },
    { label: '???? ѕжб«— ”дџ«Ёж—н (SGD)', value: Currency.SGD },
    { label: '???? ѕжб«— еждџ яждџ (HKD)', value: Currency.HKD },
    { label: '???? жжд яж—н (KRW)', value: Currency.KRW },
    { label: '???? —н«б »—«“нбн (BRL)', value: Currency.BRL },
    { label: '???? »н“ж гя”нян (MXN)', value: Currency.MXN },
    { label: '???? —ж»б —ж”н (RUB)', value: Currency.RUB },
    { label: '???? я—жд… ”жнѕн… (SEK)', value: Currency.SEK },
    { label: '???? я—жд… д—жнћн… (NOK)', value: Currency.NOK },
    { label: '???? я—жд… ѕдг«—ян… (DKK)', value: Currency.DKK },
    { label: '???? “бж н »жбдѕн (PLN)', value: Currency.PLN },
    { label: '???? »«   «нбдѕн (THB)', value: Currency.THB },
    { label: '???? »н“ж Ёб»ндн (PHP)', value: Currency.PHP },
    { label: '???? ѕждџ Ён д«гн (VND)', value: Currency.VND },
];
