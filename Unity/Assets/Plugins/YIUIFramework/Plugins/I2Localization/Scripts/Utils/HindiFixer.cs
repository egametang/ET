using System.Linq;

namespace I2.Loc
{

    public class HindiFixer
    {

        // Needs to also implement: Hindi: https://www.microsoft.com/typography/OpenTypeDev/devanagari/intro.htm
        //https://social.msdn.microsoft.com/Forums/windows/en-US/9883ff08-bd72-499b-9543-ed424167281d/converting-hindi-text-to-english-text?forum=winforms
        internal static string Fix(string text)
        {
            while (true)
            {
                char[] arr = text.ToCharArray();
                bool changed = false;

                for (int i = 0; i < arr.Length; ++i)
                {
                    // interchange the order of "i" vowel
                    if (arr[i] == 2367 && !char.IsWhiteSpace(arr[i - 1]) && arr[i - 1]!=0)
                    {
                        arr[i] = arr[i - 1];
                        arr[i - 1] = (char)2367;
                        changed = true;
                    }

                    if (i == arr.Length - 1)
                        continue;

                    // letter "I" + Nukta forms letter vocalic "L"
                    if (arr[i] == 2311)
                    {
                        if (arr[i + 1] == 2364)
                        {
                            arr[i] = (char)2316;
                            arr[i + 1] = (char)0;
                            changed = true;
                        }
                    }

                    // vowel sign vocalic "R" + sign Nukta forms vowel sign vocalic "Rr"
                    if (arr[i] == 2371)
                    {
                        if (arr[i + 1] == 2364)
                        {
                            arr[i] = (char)2372;
                            arr[i + 1] = (char)0;
                            changed = true;
                        }
                    }

                    // Candrabindu + sign Nukta forms Om
                    if (arr[i] == 2305)
                    {
                        if (arr[i + 1] == 2364)
                        {
                            arr[i] = (char)2384;
                            arr[i + 1] = (char)0;
                            changed = true;
                        }
                    }

                    // letter vocalic "R" + sign Nukta forms letter vocalic "Rr"
                    if (arr[i] == 2315)
                    {
                        if (arr[i + 1] == 2364)
                        {
                            arr[i] = (char)2400;
                            arr[i + 1] = (char)0;
                            changed = true;
                        }
                    }

                    // letter "Ii" + sign Nukta forms letter vocalic "LI"
                    if (arr[i] == 2312)
                    {
                        if (arr[i + 1] == 2364)
                        {
                            arr[i] = (char)2401;
                            arr[i + 1] = (char)0;
                            changed = true;
                        }
                    }

                    // vowel sign "I" + sign Nukta forms vowel sign vocalic "L"
                    if (arr[i] == 2367)
                    {
                        if (arr[i + 1] == 2364)
                        {
                            arr[i] = (char)2402;
                            arr[i + 1] = (char)0;
                            changed = true;
                        }
                    }

                    // vowel sign "Ii" + sign Nukta forms vowel sign vocalic "LI"
                    if (arr[i] == 2368)
                    {
                        if (arr[i + 1] == 2364)
                        {
                            arr[i] = (char)2403;
                            arr[i + 1] = (char)0;
                            changed = true;
                        }
                    }

                    // Danda + sign Nukta forms sign Avagraha
                    if (arr[i] == 2404)
                    {
                        if (arr[i + 1] == 2364)
                        {
                            arr[i] = (char)2365;
                            arr[i + 1] = (char)0;
                            changed = true;
                        }
                    }

                    // consonant + Halant + Halant + consonant forms consonant + Halant + ZWNJ + consonant
                    //if (arr[i] == 2381)
                    //{
                    //    if (arr[i + 1] == 2381)
                    //    {
                    //        arr[i+1] = (char)8204; //
                    //    }
                    //}

                    // consonant + Halant + Nukta + consonant forms consonant + Halant + ZWJ + Consonant
                    //if (arr[i] == 2364)
                    //{
                    //    if (arr[i + 1] == 2381)
                    //    {
                    //        arr[i] = (char)2381; //
                    //        arr[i+1] = (char)8205; //
                    //    }
                    //}
                    /*if (arr[i] == 0x938 && arr[i + 1] == 0x94D)//थ')
                    {
                        arr[i] = (char)0x930;
                        arr[i + 1] = (char)0;
                    }*/
                }

                if (!changed)
                {
                    return text;
                }

                var newText = new string(arr.Where(x => x != 0).ToArray());
                if (newText == text)
                    return newText;
                text = newText;
                return text;   // remove this later to allow for several passes
            }
        }
    }
}
