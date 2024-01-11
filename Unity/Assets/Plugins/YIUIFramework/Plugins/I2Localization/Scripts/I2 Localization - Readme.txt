----------------------------------------------
              I2 Localization
                  2.8.20 f2
        http://www.inter-illusion.com
          inter.illusion@gmail.com
----------------------------------------------

Thank you for buying I2 Localization!

Documentation can be found here: http://www.inter-illusion.com/assets/I2LocalizationManual/I2LocalizationManual.html

A few basic tutorials and more info: http://www.inter-illusion.com/tools/i2-localization

If you have any questions, suggestions, comments or feature requests, please
drop by the I2 forum: http://www.inter-illusion.com/forum/index

----------------------
  Installation
----------------------

1- Import the plugin package into a Unity project.
2- Enable the support for third party plugins installed on the project 
   (Menu: Tools\I2 Localization\Enable Plugins)
3- Open any of the example scenes to see how to setup and localize Components
   (Assets/I2/Localization/Examples)
4- To create your own localizations, open the prefab I2\Localization\Resources\I2Languages   
5- Create the languages you will support.
6- The I2Languages source is a global source accessible by all scenes

The documentation provides further explanation on each of those steps and some tutorials.
Also its presented how to convert an existing NGUI localization into the I2 Localization system.


-----------------------
PLAYMAKER
-----------------------

If you use PlayMaker, please, install:
   - http://www.inter-illusion.com/Downloads/I2Localization_PlayMaker.unitypackage
   - PlayMaker Unity UI Addon  (from the Playmaker website - only needed for the example scenes that use Unity UI)

That will install custom actions to access and modify the localization.
More details can be found in the example scene 
    Assets\I2\Localization PlayMaker\Playmaker Localization.unity


-----------------------
 Ratings
-----------------------

If you find this plugin to be useful and want to recommend others to use it. 
Please leave a review or rate it on the Asset Store. 
That will help with the sales and allow me to invest more time improving the plugin!!

-----------------------
 Extras
-----------------------

These are some of my plugins that may help you develop a better game:

I2 Parallax        - Uses the mobile gyroscope to add depth to your UI                      (http://bit.ly/I2_Parallax)
I2 Text Animation  - Take your texts to the next level                                      (http://bit.ly/I2_TextAnimation)
I2 MiniGames       - A better way to do Rewards and Energy Systems                          (http://bit.ly/I2_MiniGames)
AssetStore Deals   - A bot that constantly checks the Store to find you the latest Sales    (http://deals.inter-illusion.com)

-----------------------
 Version History
-----------------------
2.8.20
NEW: Missing translations are now shown in the console log.
FIX: Null Exception when running in 2021.3

2.8.19
NEW: Baking terms now supports the use of Chinese/Japanese characters
FIX: Calling Import_Google with force=true, was not using the force parameter and skipping importing the data
FIX: Url to download the Playmaker actions was not right.

2.8.18
NEW: Translations fallbacks now working with multiple language sources 

2.8.17
NEW: When generating ScriptLocalization.cs, if a term name is a C# keyword the variable now is set to start with @
NEW: There is now LocalizationManager.CustomApplyLocalizationParams to process ALL translations before ApplyLocalizationParams runs
FIX: Translations with RTL tags were failing (thanks @d xy for looking into this)

2.8.16
FIX: TextMesh Pro was not correctly displaying multi-line arabic texts
 
2.8.15
NEW: Performance improvement when switching levels by not calling Resources.UnloadUnusedAssets() unless the user requests it. 
     For most common scenarios, there is no need to unloadUnusedAssets
NEW: Lots of stability fixes by cleaning up all warnings detected by the Roslyn Analyzers
NEW: Fixed several issues/obsolete calls when on the latest versions of Unity       

2.8.14
NEW: Added support for localizing Video Player
NEW: Allow skipping Google Synchronization from script by creating a RegisterCallback_AllowSyncFromGoogle class 
     (http://www.inter-illusion.com/forum/i2-localization/1679-turning-off-auto-update-via-script#4166)
FIX: UI changes to remove dark colors in the background     
FIX: Disabling Localized Parameters now work as expected (i.e. Parameters still work, but they are not localized even if a term is found with that value)
FIX: Target TextMesh now correctly changes font and materials 
     
     
2.8.13
NEW: Added UpdateFrequency EveryOtherDay to allow checking the spreadsheet every 48h
NEW: Added a function to retrieve an asset by using the term name (e.g. font, sprite) LocalizationManager.GetTranslatedObjectByTermName<t>(TermName)
FIX: Replaced all events using Action<..> by delegate function that clearly show what the parameters are for
FIX: Inspector was not showing correctly when in the latest alpha/beta of Unity 2019.2+
FIX: Bug where Languages coudn't be enabled at runtime  (thanks to bschug for submitting a fix)
FIX: To reduce memory usage, Term descriptions are now only used in the editor (not in the final build) If needed, use a disabled language to store it
FIX: Inspector Width should now be detected more accurately


2.8.12
NEW: Now is possible to dynamically create a LanguageSourceData without a LanguageSource and use it at runtime.
NEW: LocalizationParamsManager now have a Manager Type (Local/Global) to define if it applies to ALL Localize or just the one in the same object.
FIX: Term's list is not longer fully expanded all the time.
FIX: Specialization bar in the Term's translations now shows the selected specialization as a tab-button
FIX: Adding new specializations now copies the translation from the current specialization, so the user can create several in sequence.
FIX: Terms can now have any letter, digit or any of the following symbols .-_$#@*()[]{}+:?!&'`,^=<>~
FIX: Removed invalid character from the StringObfucator that were causing build issues in XBox
FIX: RTL Fixer (Arabic, Persian, etc) was detecting tags like <xx>, only [xx]


2.8.11
NEW: Term's name can now have any of the following symbols ".-_$#@*)(][}{+:?!&'"
NEW: Now the plugin localizes MeshRenderer  (Mesh + Material)
NEW: New parameter in the Google Spreadsheet tab to control when to apply the downloaded data (OnSceneLoaded, Manual, AsSoonAsDownloaded)
FIX: Inferring terms with '/' no longer generates a Category
FIX: Updated NGUI target to be compatible with the latest version
FIX: Localize inspector, the options in the dropdown for "ForceLocalize" and "Allow Localized Parameters" were in the wrong order
FIX: Inspector for the Google Spreadsheet tab in the LanguageSource was cutting when the Inspector was too narrow
FIX: Button "Open Source" in the Localize Inspector now works for both LanguageSources and LanguageSourceAssets
FIX: Terms name can now have Non latin letters (e.g. Chinese, Korean, etc), but no symbols, extra spaces, etc

2.8.10
NEW: LocalizationManager.GetCurrentDevice(true) will now force get the Device Language without using the startup language from the cache
FIX: Updated Playmaker actions to new Prefab system
FIX: Menu option: Tools/I2 Localization/Open I2Languages
FIX: When inferring terms, any tag (e.g. <color>) or invalid characters (e.g. ^/\`) are removed to form the term name

2.8.9
NEW: I2Languages.prefab is now an ScriptableObject instead of a prefab to avoid the locking issues of Unity 2018.3
FIX: Compatibility with Unity 2018.3
FIX: Plural rules for some Slavic languages were using the wrong settings.
FIX: Improved performance when using assets from Resources and in general only calling Resource.UnloadAll when loading scenes

2.8.8
NEW: Added a flag to disable Localized Parameters 
     (http://inter-illusion.com/forum/i2-localization/1163-localization-params-auto-translating-design-flaw#3195)
NEW: Merged all checkboxes in the Localize component into an "Options" popup to make the Localize's inspector use less space
NEW: Find all terms in the Scene/Scripts now also detects those in your code using [TermsPopup] and LocalizedString
FIX: Changing the Font or Material in a TextMeshPro now updates the linkedTextComponent
FIX: Startup language now ignores the disabled Languages if the "Default Language" is set to "Device Language"
FIX: Updated Playmaker example scene to show that it needs the Unity UI Addon.
FIX: Prevented a null reference on the GetSourcePlayerPrefName function when using Google Live Synchronization in Scene's sources
FIX: Google Live Synchronization was failing to load the downloaded data from the cache.

2.8.7
NEW: WebService now has a password to restrict other players from modifying the localization data
NEW: Importing a CSV file from script will now update the temporal files while playing
NEW: A warning is now displayed in top of the LanguageSource when Google Live Sync is enabled but the spreadsheet is not up-to-date
NEW: New option in the Languages tab to define if Runtime Unloading of Languages happens for the source, or if only in the device
NEW: By default, the Runtime Unloading of language will not happen in the Editor (to allow for editing while playing)
NEW: Missing terms are going to show hidden by default, given that they are only suggestions and are making the list to crowded
NEW: Added function LocalizationManager.GetTranslatedObject(termName) to return the translated Sprite/Material/Font/etc from a Term
FIX: Translating and editing terms now keep the [i2nt]...[/i2nt] sections correctly
FIX: Translating texts with rich text tags was removing the tags
FIX: Modified temporal language file format to recover from errors and avoid shifting the term's translations
FIX: When source is set to show a warning when the term is not found, the Localized Parameters was showing the warning
FIX: Corrected wrong layout in Terms list when in Retina displays (contributed by @TailoraSAS)
FIX: Languages will fallback were filling their translations when doing a build

2.8.6
NEW: Menu "Tools/I2 Localization/Toggle Highlight Localized" changes all localized text into "LOC:<TermName>" to easily spot errors
NEW: "Bake Terms" tool now generate another class (ScriptTerms) in ScriptLocalization.cs that class have variables for the Term Names
NEW: Parameters can now be localized if the parameter value is the name of an existing Term
NEW: Terms filtering in the LanguageSource list, now allow prefix 'f xxx' or 'c xx' to search in translations or category
FIX: Downloaded Spreadsheets will also save their key to avoid corrupting the cache when running different versions of the same app
FIX: Google Live Synchronization was not updating the temporal languages files used to save memory at runtime
FIX: GetCommonWordInLanguageNames now does a Case Insensitive comparison ("English (Canada)" can now match "english")
FIX: Latest unity beta was failing to detect TextMeshPro when installed using the PackageManager
FIX: Removed warning regarding Tizen
FIX: Clicking the Language names in the Term's description was not previewing the translations
FIX: Changing Fonts/Objects and Localize Targets when selecting multiple Localize components was only updating one of them
FIX: Charset tool was not removing the last ] from tags
FIX: ParameterManager activation was crashing IOS build in some Unity versions

2.8.5
NEW: Once a day (can be configured), the editor will check if the Spreadsheet is up-to-date to avoid issues when playing in device
NEW: Added Language code 'es-US' to support "Spanish (Latin Americas)"
NEW: Added Toggle "Separate Specializations into Rows" to Spreadsheet export inspector, to either merge or split the specializations
NEW: Startup language will now try to match an official language before trying a fallback to any custom language name or variant
FIX: Fallback languages now try finding a language of the same country, and then fallback to the first from the list
FIX: Removed harmless logs marked as error related to File not found or accessible
FIX: Disable language Loading/Unloading on the Switch console
FIX: Some character combinations where producing an error when using the CharSet tool and clicking "Copy to clipboard"
FIX: When the starting language had Fallbacks, those where not loaded correctly
FIX: If the Localize or LanguageSource inspector was open, changing the languages was not updating the inspector preview
FIX: Removed example script using OnMouseUp to avoid showing a warning when building the game (replaced with Unity UI)

2.8.4
NEW: Component CustomLocalizeCallback with a UnityEvent to set functions that should be called whenever the Language changes
NEW: Localize component now has the Callback as a UnityEvent which allows calling several function and even passing parameters
NEW: Customizable PlayerPrefs and FileAccess, allowing setting your own functions to handle settings and file IO
NEW: Term's list can now show and filter more than 31 categories (contributed by @71M THANKS!)
NEW: LocalizationManager.CurrentCulture to allow formating (e.g. string.Format(LocalizationManager.CurrentCulture, "{0:c}", 12))
FIX: Tool CharSet will no longer add Plural tag characters (e.g. [i2p_Zero])
FIX: Refactored BundlesManager to allow implementing a CustomBundlesManager
FIX: Translating a text with a plural parameter (e.g. {[#POINTS]} ) now generate all plural forms in the target language
FIX: Android devices were not auto-detecting some languages on startup 
FIX: Translations that starts with characters (') or (=) can now be correctly exported and imported into Google Spreadsheets
FIX: Localize Target was not detected in Unity 2018+
FIX: Building XCode with Append failed unless set to Replace or manually deleting the Localization files

2.8.3
NEW: Runtime Memory optimization by loading/unloading languages depending on which ones is in use
NEW: Added Support for the following languages: (Although Google Translate doesn't support all of them)
     Abkhazian, Afar, Akan, Amharic, Aragonese, Assamese, Avaric, Avestan, Aymara, Bambara, Bashkir, Bengali, Bihari,
     Bislama, Breton, Burmese, Chamorro, Chechen, Chichewa, Chuvash, Cornish, Corsican, Cree, Divehi, Dzongkha, Ewe,
     Fijian, Fulah, Guaraní, Haitian, Hausa, Herero, Hiri Motu, Interlingua, Interlingue, Igbo, Inupiaq, Ido, Inuktitut,
     Javanese, Kalaallisut, Kanuri, Kashmiri, Central Khmer, Kikuyu, Kinyarwanda, Kirghiz, Komi, Kongo, Kuanyama,
     Luxembourgish, Ganda, Limburgan, Lingala, Lao, Luba-Katanga, Manx, Malagasy, Marshallese, Nauru, Navajo, North Ndebele,
     Nepali, Ndonga, Sichuan Yi, South Ndebele, Occitan, Ojibwa, Church Slavic, Oromo, Oriya, Ossetian, Panjabi, Pali, Rundi,
     Sanskrit, Sardinian, Sindhi, Northern Sami, Samoan, Sango, Scottish Gaelic, Shona, Sinhala, Somali, Southern Sotho,
     Sundanese, Swati, Tajik, Tigrinya, Tibetan, Turkmen, Tagalog, Tswana, Tonga, Tsonga, Twi, Tahitian, Uighur, Venda,
     Volapük, Walloon, Wolof, Frisian, Yoruba, Zhuang
NEW: Exposed a variant of the LocalizationManager.ApplyLocalizationParams that can get the parameters from a function
NEW: Localization Target now use ScriptableObject to allow keeping extra parameters to setup the targets
NEW: Android devices now properly detect the Device Language Region (no longer using Unity's Application.SystemLanguage)
FIX: Two parameters in a single translation was failing if the length of the text was not the same
FIX: 'Missing' icon was not showing in the Term's list when in Unity 2017+
FIX: Compatibility with Window Store Apps and UWP
FIX: Support for multiple Localize component in the same GameObject
FIX: Increased the PostProcessBuild priority to avoid conflicting with other plugins
FIX: Assigning None to a Term translation's Object Field was ignored
FIX: LocalizationManager.OnLocalizeEvent now correctly releases all callbacks after finishing Play Mode in the Editor
FIX: Term list in language source was showing empty space for some seconds after unselecting a term

2.8.2
NEW: Improved the example scene: 'I2Localization    features LocalizedString.unity'
NEW: Added a version of ForceTranslate to translate several texts at the same time
NEW: The translate/Translate All button will now skip terms that don't have type "Text" (avoid Materials, Fonts, etc)
NEW: Restored the translate button for each translation field, but this time it shows only "T" to still allow more space
NEW: Google Translate will now skip tags (e.g. [tag]..[/tag], <tag>..</tag>)
NEW: Google Translate skips any text inside [i2nt].ignored.text.[i2nt].  Those tags are also not used when rendering translations
NEW: Basic Hindi / Devanagari support
NEW: Optimized GC Allocations, removed runtime usage of Regex
NEW: By default, all scenes will be selected in the LanguageSource tools (Parsing, Renaming, Changing Category, etc)
FIX: Issue with some unity versions failing when Translate All/Export with an error related to 'Rewinding' the POST result
FIX: Issue where clicking the translate button was failing when terms source translation had & or similar symbols
FIX: Issue with some unity versions failing when Translate All/Export with an error related to 'Rewinding' the POST result
FIX: Issue where clicking the translate button was failing when terms source translation had & or similar symbols
FIX: Button "Translate All" will not longer override non-empty translations when input type is Touch instead of Normal
FIX: Play in Editor sometimes failed to localize when changing languages
FIX: Translating all terms to a language with a variant (e.g. en-CA) was failing
FIX: Example scenes had warning loading when the project was set to serialization: Force Text 
FIX: Long Key names are now clamped to 100 characters in the terms list so that it doesn't get too wide
FIX: GoogleTranslate.ForceTranslate was not handling tags and parameters
FIX: LanguageSources in the scenes were not loaded inside the editor until they were clicked
FIX: Localize target AudioSource was not playing all the time
FIX: Selecting a different Target type in the Localize component was not updating correctly
FIX: Localization failed when there where empty slots in the Assets list (LangaugeSource) or References (localizeComponent)

2.8.1
NEW: Plural support with multiple Plural forms based on the target language
NEW: LocalizeDropdown now supports TMPro Dropdown
NEW: Adding a Term will automatically detect the Term type (e.g. Sprite, Font,...) (instead of always defaulting to "Text")
NEW: Google Translation will now generate translation for each plural form of the target language
NEW: New Term Type: Child  (it enables the child GameObject with the name matching the translation to that language)
NEW: CharSet tool now has buttons to select all languages, clear or invert the selection
NEW: Right-To-Left and Modifiers sections in the Localize inspector will now only show if the term's type is 'Text'
NEW: Confirmation dialog before deleting a Language
NEW: Modified the WebService to support translation requests using POST. This increases the reliability of translating large data sets.
NEW: Added a close button to the Error message in the inspector.
NEW: When the Verify WebService fails, it will now display the error in the inspector
NEW: LocalizationManager.ApplyLocalizationParameters can now use Global, Local parameters or a dictionary of parameters
NEW: LocalizedString inspector now has a button that opens the LanguageSource to allow editing the term
NEW: The LanguageSource now has an option to decide if default Language will be Device Language or the first in the Languages list
NEW: LanguageSource's terms list now shows 3 dots (...) after the last term if some terms were hidden by the category filter
NEW: Sub Sprite Terms will now show the full path of the sprite (e.g. "Atlas.SpriteName")
DEL: Removed the "Translate" button next to each translation to allow for more space. Instead, use the "Translate All" in term/language
FIX: Term Type 'GameObject' now works as expected
FIX: IOS AppName Localization was not exporting languages with regions
FIX: Clicking the delete button ("x") of a disable button will now correctly delete it (no need to manually enable and then delete)
FIX: Google Translation will not longer fail silently when the target language is not supported by Google translate
FIX: Building Android Apps with a name including (') was failing
FIX: Sometimes when playing in editor, changing the language didn't localize all texts
FIX: Restored the GoogleTranslation.ForceTranslate (although its noted that it may fail in some unity versions having the www blocking bug)

2.8.0
NEW: Downloaded Spreadsheets at runtime are now saved into a file in the persitentData folder to avoid overflowing the PlayerPrefs
NEW: Example scene showing how to use LocalizedString
NEW: Localize component now has a checkbox (Separate Spaces) to add extra spaces to the languages that don't add them between words.
NEW: All saved translations are now encrypted by default, feel free to change the password in StringObfucator.cs for added security
NEW: ScriptLocalization is no longer built by default (to speed out compilation by avoiding checking all the time)
NEW: Instead of ScriptLocalization.Get(xx), it will be better to use LocalizationManager.GetTranslation(xx)
NEW: TermsPopup attribute now has an optional filter (e.g. [TermsPopup("Tutorials")] ). This change was contributed by @michael THANKS!
NEW: LocalizationManager.GetTranslation(Term, overrideLanguage="English") allows retrieving translations outside the current Language.
NEW: Tool PARSE now also detect script reference to LocalizationManager.TryGetTranslation("term")
NEW: Importing terms from Google will now remove invalid (Non-ASCII) characters from the Terms Name
NEW: Selecting the Language Code in the Languages Tab, now shows a more compact list with languages and variants as childs
NEW: When the localize component has a LanguageSource set as Source, it will only show the terms inside that source
NEW: Added a "Detect" button after the Source selection in the Localize component to find the LanguageSource containing the selected term
NEW: Added a confirmation dialog to the delete terms button
DEL: Deprecated old TextMeshPro_Pre53
DEL: Removed support for DFGUI (was removed from the AssetStore a while ago, if you still need support, please use I2Loc v2.7.0)
FIX: Localization targets are now AutoRegistering before the Awake function of the scene. This fixes issues with the LocalizeOnAwake flag
FIX: Export to Android was failing to create the correct Locale folders (i.e. values-en-rUS  instead of values-en-US)
FIX: TextField used to filter the Terms List in the LanguageSource was not allowing Copy/Paste
FIX: Term's name can now include non-ASCII character as long as they are not controls (e.g. newline, tab, etc)
FIX: Android XML with the AppName will now properly escape the name if it contains XML-ilegal characters
FIX: Compatibility update for Unity 2017 when editor is set to .Net 4.6  (thanks to @nox_bello)
FIX: Tool PARSE was not detecting the correct term when ScriptLocalization.Get("xx", param1, param2) had 1 or more params
FIX: In some unity versions, compiling for UWP was throwing an exception when Google Live Sync was used.
FIX: Exporting Chinese Variants in ANDROID was failing because Android uses (zh-rXX instead of zh-XX) [thx to @fur contribution]
FIX: Disabling the last language in the LanguageSource list, was disabling some other UI controls outside that language
FIX: Realtime Translation example was failing when the I2Languages.prefab was not selected
FIX: TextField used to filter the Terms List in the LanguageSource will now not lose the focus when typing
FIX: When clicking Translate or Translate All int the editor, it was not showing the "Translating...." message
FIX: Translating texts with & was return wrong translations
FIX: Sometimes the inspector width was bigger than it had to be


2.7.0
NEW: To separate purposely empty translations from missing ones, the empty translations should be set to ---
NEW: Google Update Frequency can now be set to "OnlyOnce" to only download data from the Spreadsheet the first time the app is executed
FIX: OnMissingTranslation in the LanguageSource now is working as expected
FIX: Compatibility with Unity2017 was causing the editor to not find some EditorStyles and the Translate function was looping forever
FIX: Changing text alignment will not be reverted when switching languages
DEL: ForceTranslate is now removed, use the GoogleTranslation.Translate instead (because Unity 2017 doesn't run www in a separate thread)

2.6.12
NEW: When a term is not found, its translation will be null instead of "" to diferenciate that case from terms with empty translations
FIX: Terms with empty translations will change the label's text to empty.
FIX: Internal term "-" will not longer be shown in the LanguagesSources
FIX: Several errors when LanguageCode was invalid or only had 1 letter
FIX: Renaming terms in multiple scenes will now correctly save the scenes
FIX: Clicking the inspector to preview a language and then exiting the Localize inspector was only reverting the selected object
FIX: Corrected "RightToLeft Text Rendering" example scene (added names to the labels and made the buttons select the correct language)
FIX: RTL Line Wrap (when maxCharacters>0) will not longer add extra lines at the bottom
FIX: Language Source was downloading data from google when any delay was specified (even when the frequency was set to NEVER)
FIX: Building android on some MAC was failing to resolve the staging path
FIX: RTL languages will now be fixed correctly when surrounded by [] and other simbols
FIX: Compile error (about SceneManager) when running the game in 5.2 or older
FIX: Google Live Sync now will download the data, even if the scene is changed while the spreadsheet is been downloaded
FIX: New TextMeshPro not been detected because the DLL changed its signature


2.6.11
NEW: App Name can now be localized in both IOS and Android
NEW: Localize term now has two string fields to add a Prefix & Suffix to the translations (e.g. allows adding : at the end of the text)
NEW: Example of how to change the language using a Unity UI Dropdown (component SetLanguageDropdown) see the "uGUI Localization" Scene
FIX: IOS Store Integration now works automatically
FIX: Parsing terms was not updating the Term's Usage
FIX: Better support for arabic texts with tags
FIX: Previewing a language in the editor by clicking the Language name, will now respect the RTL or LTR direction depending on the language
FIX: Removed warnings in Unity 5.6
FIX: TextMeshPro not updating the font when using a material name in a subfolder of the Resources 
FIX: TextMeshPro now wraps correctly texts from Right-To-Left Languages (e.g. Arabic)
FIX: CharSet tool will now correctly find what Arabic and other RTL characters are used (will apply RTLfix when adding the characters)
FIX: TextMeshPro (paid version, the one with the source code) was not been detected correctly
FIX: Translating a term inferring from Label's text, will no longer remove the non ASCII characters
FIX: Google Live Sync will now auto-update the scene texts without needing to change the current language
FIX: LocalizationManager.GetTermData will now call InitializeIfNeeded, which also now validates that there are sources available
FIX: Added validation for undefined BuildTargetGroup.Switch in 5.6

2.6.10
NEW: PlayMaker support (Actions: Get/SetCurrentLangauge, SetNextLanguage, Get/SetTerm, GetTranslation)
NEW: Button next to the plurals tab to show if the DisabledLanguages should show the translation or should be hidden
NEW: Massive inspector speed improvement when Parsing terms, selecting a LanguageSource, etc
NEW: LocalizationManager.ApplyLocalizationParams now accepts a general GameObject instead of forcing a LocalizeCmp. This allows using LOCAL parameters without localize component
NEW: added options for parameters when calling ScriptLocalization.Get(term, true, 0, false, applyParameters:true, localParamsRoot:gameObject)
NEW: Clicking a Translation in the Localize component will now Preview that language in the entire UI
NEW: Added a button at the top of the Term's List in the LanguageSource to refresh the translation shown in the Scene (calls LocalizeAll(true))
NEW: Menu option to also call the LocalizeAll (Menu/I2/Localization/Refresh Translations). This is useful if the translation is changed and should be updated in several UI elements
NEW: LocalizationManager.GetTermsList() now can have a parameter to only show the terms of that category (e.g. .GetTermsList("Tutorial"))
NEW: LocalizationManager.HasLanguage(..) and .GetAllLanguages(..) now has optional parameter SkipDisabled to skip disabled languages. Default=true
NEW: Build ScriptLocalization.cs tool now has a button to select the terms previously built
FIX: Disabling a language will skip it when selecting the startup language
FIX: Selecting a different LanguageSource will not longer set the categories to None, instead it will revert to Everything
FIX: Now the Scene can preview the translation of disabled languages. Just click in the language name next to the Terms's description (localize component OR language Source)
FIX: Compile errors when building for Windows Phone or WSA
FIX: Generating the files for the Store Integration (Android, IOS) was also exporting the disabled languages
FIX: Alignment option for TextMeshPro now works with the latest TextMeshPro version (v1.0.55.52 Beta 3)
FIX: LocalizationManager.LocalizeAll is now internally handled at the end of the frame (using a coroutine) to handle multiple localization in the same frame and timing issues when start Playing
FIX: A term will no longer show as missing translation (yellow + italics) if the missing translation belongs to a disabled language
FIX: ResourceManager.CleanResourceCache will now only be called when a level is loaded to avoid frame rate spikes in the middle of the game
FIX: Added support for the new TextMeshPro (free version)


2.6.9
NEW: AboutWindow will never open automatically, instead there will be an small warning next to the version number
NEW: LocalizedString  (link to documentation)
NEW: Changing .NET CurrentCulture when the language changes is now optional. Its disabled by default and can be enabled by adding an AutoChangeCultureInfo component to your first scene
NEW: Terms can now be set to <none> to avoid localizing it (e.g. Don't localize Label's text, but do localize Label's font)
NEW: Languages can now be disabled (useful for having data columns in the spreadsheet) Just add a $ in front of the language name in the Spreadsheet or uncheck the toggle in the LanguageSource
NEW: Clicking the Language name next to the translations will preview the text in that language
FIX: Event_OnSourceUpdateFromGoogle is now called with ReceivedNewData==false whenever the languagesource is up-to-date
FIX: Auto-detection of plugins was not working correctly for IOS
FIX: Removed warning "Unsupported encoding: 'UTF-8,text/plain'" that was happening when translating a text
FIX: RTL fix was not been applied to Right-To-Left languages when EnableChangingCultureInfo was not called or false
FIX: Changing Category was not marking the scene Dirty

2.6.8
DEL: [i2auto] will not longer be used (if your spreadsheets have that, you should remove all [i2auto] texts and reimport to the LanguageSource)
DEL: Translations will not longer be marked as translated by google (this speed the import/export process)
NEW: When a term is not defined for some language, it can be set to display (Empty, Fallback or a Warning) The setting is in I2Languages.prefab Language Tab
NEW: Localize components will execute the Localization Callback even without a valid Translated Term.
NEW: When creating a term in the Localize component, the first language (e.g. English) will be auto-filled with the label's text
NEW: Added two more parameters to the callback when Import_Google finishes (Event_OnSourceUpdateFromGoogle(LanguageSource, bool ReceivedNewData, string errorMsg))
NEW: Selecting the checkbox next to the Term's list in the LanguageSource and then clicking the Term Usage button selects ALL objects using all the selected terms
NEW: Surrounding a text with the tag <ignoreRTL>, ignores converting it to RTL  (e.g. "<ignoreRTL>2<ignoreRTL>. بدفع مبلغ")
NEW: New toggle (Ignore Numbers), next to the Max Line Length in the localize component, to automatically avoid converting numbers when parsing RTL texts
NEW: .NET CurrentCulture is changed based on the current language to make all culture-dependant operations to use the properties of the selected language
NEW: Detecting if the language is Right-To-Left will now be using the CurrentCulture settings as that maybe more precise
FIX: Right-To-Left languages will not adjust the alignment if the original alignment was CENTER
FIX: 2DToolkit now allows adjusting the alignment if the language is RTL
FIX: Add/Remove language was not marking the LanguageSource as dirty and the changes could have been getting lost
FIX: Copy/Paste a Localize component into a new GameObject will properly update the Target reference
FIX: If the WebService was set in a LanguageSource inside the scene and not in the I2Languages.prefab, Google Translate/Export/Import wasn't working.
FIX: Compile erors when using an old version of TextMeshPro (requiring TextMeshPro_Pre53)
FIX: Texts for Right-To-Left languages containing multiple lines was showing extra lines when using \r\n for new lines
FIX: LanguageSourceData.Import_Google was not executing when Auto-Update was set to NEVER  (even if ForceUpdate was true)
FIX: Accessing www.text was returning an Encoding error in the latest patch releases (5.4.1p3 and 5.3.5p7)

2.6.7
NEW: SpriteRenderer can now be localized
NEW: Translations can have Parameters (e.g. "The winner is {[WINNER}]") and at runtime the tag is replaced by its value by using a local or global parameter
NEW: Local parameters can be set by adding a LocalizationParamsManager component to the gameObject, and it has a list of parameters (i.e. <Name, Value> pairs)
NEW: Global parameters can be set by adding a ILocalizationParamsManager class to the LocalizationManager.ParamsManager list
NEW: Example Scene named "Callbacks and Parameters" showing how to modify the translations using Callbacks, Local Parameters and Global Parameters
NEW: Unity UI Dropdowns can be localized by adding the LocalizeDropdown component
NEW: Localized objects (Textures, sprites) can now be loaded from a bundled by registering a ResourceManager_Bundles (see RegisterBundlesManager.cs example)
NEW: ScriptLocalization.Get(term)  will now automatically fix it for RTL if the current language is Right-To-Left (use .Get(term, false) to avoid that)
NEW: Parse Terms in Scripts will now match terms in the form "LocalizationManager.GetTranslation" as well as "ScriptLocalization.Get"
NEW: ScriptLocalization.cs is now autogenerated to avoid overriding existing localizations. That file is also now moved into the Assets\I2 folder
NEW: Localize component now has a toogle "Force Localize" that should be true when the translation has parameters to force the localization when the object is enabled
NEW: Tool CharSet now has a button "Copy To Clipboard"
NEW: Local parameters SetParameterValue function now has an optional parameter to skip the localization (useful when setting several parameters in a frame)
NEW: LocalizationParamsManager.OnLocalize() executes the localization with the parameters previusly set.
FIX: Auto-Sync from Google Spreadsheet was not detecting the new versions correctly and wasn't loading from the cache.
FIX: Right-To-Left texts will now correctly handle ritch-text tags (e.g. <color=red>..</color> and [FF0000]..[-], etc)
FIX: Expanding/Collapsing the Terms, References or OnLocalize Callback in the Localize component is now remembered
FIX: Changing <none> in the terms selection list by <inferred from text> as thats more understandable
FIX: Selecting <inferred from text> from the list of term will use the Inferred one.
FIX: Selected Term is now drawn in a light Yellow when it is inferred (previously was dark yellow and wasn't as visible in the Editor Light Theme)
FIX: Renamed button at the bottom of the terms description to make it more understandable. From "Merge" to "Rename".
FIX: When clicking that Merge button, the current term is automatically selected as the term to rename
FIX: Exporting a csv file with auto-translated terms containing (,) was generating extra columns with "[i2auto]".
FIX: ParseTerms in scripts was not detecting the term when the function had spaces (e.g. ScriptLocalization.Get ( "term" ))

2.6.6
NEW: Allow multiple Localize component in the same object
NEW: TextMesh Pro localization when changing material (e.g. "ARIAL SDF - Outline") will now also find and use the corresponding font (e.g. "ARIAL SDF")
NEW: Added a Delay to the Auto-update from Google to wait some time before updating. To prevent a lag on startup
NEW: Exporting to a spreadsheet will sort the terms
NEW: Charset Tool allows adding upper and lower versions of characters even when one character variant is not found
FIX: Empty languages can not longer be added by clicking the "Add" button
FIX: Columns with empty language name in Google Spreadsheet or CSV files are now skipped
FIX: Sometimes when playing in the Devices, I2 Localization was using old localization data from PlayerPrefs 
FIX: Google Live Synchronization was not detecting correctly the Spreadsheet changes
FIX: Removed a debug log that was printing the entire content of the downloaded spreadsheet, making the log file hard to read
FIX: Removing a Term from the LanguageSource was still displaying it in the Terms List even though they werent there anymore
FIX: Compile warning related missing BuildTargetGroups when detecting installed Plugins
FIX: Translation of UPPERCASE texts are now handled correctly
FIX: Categories/terms matching part of another category will export correctly (e.g. TUTORIAL   and   TUTORIAL1\Welcome)
FIX: I2 About Window will not longer shown when doing a build or when in batch-mode
FIX: Texts starting with a tag (e.g. [xxx]) are now accepted (useful for NGUI color tags)

2.6.5   
  (requires a new WebService: v4)
NEW: Localize.Term = xxx  works now the same that executing Localize.SetTerm(xxx)
NEW: Importing a big Spreadsheet is 20-70 times faster than before
NEW: Added a Translate button next to each language to bulk Translate all missing terms for that language
NEW: Tool to find which characters are used in the languages (useful to create bitmap fonts)
NEW: Adding a term to a Language source without languages, will automatically create "English"
NEW: NGUI and TextMeshPro example scenes now also show changing Fonts based on the language
NEW: Viewing a big LanguageSource is now smoother even when seeing several thousands terms.
NEW: Added a dropdown menu to select the File Encoding (UTF8, ANSI, etc) of the local CSV file
NEW: Use of the WebService to get the Google Translations (previously it was a hack that parsed the google web but failed whenever google changed their look)
FIX: Translating Terms was skipping the first 2 letters
FIX: Translating text with Title Case (This Is An Example) was failing with google
FIX: Translation using Term Category (Tutorial/New Example)
FIX: Removed delay when selecting languageSources caused by the parsing of terms in scripts, now scripts are only parsed when using the Parse Scripts Tool
FIX: TextMeshPro labels will auto-size correctly when switching languages
FIX: 2D Toolkit example scene was corruptedLoca
FIX: localizeComponent: Button "Add term to Source" for a secondary Term will add the term to the source containing the primary term.
FIX: Selecting "None" as a referenced object will no longer produce a null reference exception
FIX: Errors reporting that DontDestroyOnLoad can only be called in Play mode
FIX: Errors when some referenced asset was destroyed and the plugin tried to release it

2.6.4
NEW: SVG Importer has been integrated (support for SVGImage and SVGRenderer: localizes VectorGraphic and Material)
NEW: Updated to support TextMeshPro 5.2 beta 3.1 (previous versions need to change TextMeshPro by TextMeshPro_Pre53 in the scripting define symbols)
NEW: I2Languages.prefab has been moved to I2/Resources to make update easier (just delete I2/Common and I2/Localization and import the new package)
NEW: The spreadsheet will not be auto-downloaded when running in the editor as the local language source its supposed to be the most up-to-date
NEW: Better compatibility with UnityScript (added method versions to avoid default parameters, still needs to move I2L to the Plugins folder)
NEW: Inferred terms will be changed to normal terms as soon as a matching term its found
NEW: Added variables LocalizeManager.CurrentRegion and .CurrentRegionCode to get the region part of the language (e.g. "en-US" -> "US")
FIX: When re-starting the game after downloading a modified spreadsheet, it was loading the old translations
FIX: Error shown in the console when playing the game in the editor while a Localize component its shown in the Inspector
FIX: Parsing scenes failed on Unity 5.3+ when scenes where not in Assets folder
FIX: Adding a localize term before adding a target (TMPro label, UI Text, etc) failed to get the inferred Term
FIX: Selecting a Term in the localize Secondary Terms Tab, was changing the label's text to the name of the font/atlas
FIX: TextMeshPro was producing warnings regarding materials when previewing different fonts in Editor and not in Playing mode
FIX: Marking scene dirty when Localize callback and other variables are changed
FIX: Button "Add Term To Source" in the localize inspector was sometime adding the term to the wrong LanguageSource
FIX: Selecting <inferred from text> from the term's list made that option disapear the next time the popup was openned
FIX: IOS integragion will now correctly generate Info.plist instead of info.plist
FIX: When copying a Localize component and pasting it in other object will no longer keep a reference to the previous object

2.6.3
NEW: IOS Store Integration (adds the languages to the Info.plist file)
NEW: If the localize component, can find its inferred term in a source, it will use that term and stop inferring it
NEW: When adding a term to a source, the scene is parsed and every object inferring that term will start using it
NEW: In LanguageSource inspector, button "Add Terms" and "Remove Term" will use the selected term even if it doesn't have the checkbox ticked
NEW: When auto-generating ScriptLocalization.cs, if the file was moved, the plugin finds it and regenerate it in the new location
NEW: Non printable/special characters in the Terms name are removed from inferred terms to increase legibility
NEW: On the Terms list, the buttons at the bottom (All, None, Used, Not Used, Missing) now select from the visible terms not the full list.
NEW: Button "Show" next to each Language in the LanguageSource to preview all LocalizeComponents in that language
NEW: Clicking on a translation previews how it looks, but selecting another object will now stop the preview and revert to the previus language
FIX: Compile error in TitleCase(s) when building for Windows Store
FIX: Android Store Integration was using a wrong path and now all generated files are in Plugins\Android\I2Localization
FIX: Textfield used to type the new category now allows typing \ and / to create subcategories
FIX: On the Terms list, the filters (Used, NotUsed, Missing) will now work correctly with categorized terms
FIX: Improved performance on the LanguageSource and Localize inspector. Now selecting a big languageSource its around 4 times faster

2.6.2
NEW: Plugin now supports Unity 5.3
NEW: Android Store Integration (adds strings.xml for each language so that the store detects the application is localized)
NEW: When editing a term, Translate and Translate All buttons will translate the Label's text instead of the Term name
NEW: Tool to find No Localized objects now saves the Include and Exclude filters
NEW: Added a Refresh button on top of the Terms list to quickly parse all localized objects in the scene
FIX: Alignment will not revert to "Left" when switching languages. RTL languages will still be adjusted correctly.
FIX: Parse terms was not detecting inferred terms used in Localize components that were not previously opened in the inspector
FIX: Importing spreadsheets with auto-translated terms having multiple lines was adding extra quotes.
FIX: Google translate Language code of all chinese variants was updated to the right code
FIX: Changing Term Categories or Renaming it will now update the language Source
FIX: Add button (+) after the Terms list is now always at the end of the terms, even when a term is expanded
FIX: When changing category in a term that its not in the source, it will display an error box showing why it fails
FIX: Sometimes the Resources folder failed to be created if it was previously created (when generating I2Languages.prefab)
FIX: TextField to edit the Term's description and translation has word wrap enabled to avoid expanding the inspector on long lines
FIX: Vertical scrollbar in the Terms list will now hide when all terms fit in the screen
DEL: Removed checking for installed plugins when scripts are compiled (will only happen at startup or if force from the menu)

2.6.1
NEW: Multiline texts can be fixed correctly for RTL languages by specifiying the maximum line length (Localize Inspector)
NEW: Added a checkbox to the Localize Inspector to allow changing alignment for RTL Languages (Right when RTL, Left otherwise)
NEW: Adding API for accessing translated objects: (LanguageSource and Localize).AddAsset, .HasAsset, .FindAsset
NEW: Localize.FinalTerm and .FinalSecondaryTerm are now public variables that can be used in the OnLocalizationCallback
FIX: Switched loc order of Main and SecondaryTerms to localice the text/sprite after the font/atlas was changed
FIX: Editor UI for the Terms translation was overflowing. 
FIX: Automatically Importing from Google will not longer clear the localization data
FIX: Faster startup by avoding calling LanguageSourceData.UpdateDictionary multiple times
DEL: Projects using Unity new UI no longer have to add UGUI to their Scripting Define Symbols
DEL: Projects using TextMeshPro no longer have to add TMProBeta to their Scripting Define Symbols
DEL: Cleaned some variables in the Inspectors that were not longer needed


Thanks to 00christian00 and vicenterusso for their contributions!!

2.6.0
NEW: Localize component now has a "Translate ALL" button
NEW: Term can be flagged as "translated by human" or "translated by Google Translator"
NEW: The Callback in the Localize component now show all public methods of ALL monobehaviors in the Target object
NEW: Tool 'Parse Localized Terms" now allows searching for term usage in the SCENES and in the SCRIPTS
NEW: Localize was optimized to avoid localizing every time the component is enabled
NEW: Localize has now a setting for Pre-Localize on Awake or for waiting until the object is enabled.
NEW: Downloading from google uses now a custom format instead of JSON to avoid parsing errors
NEW: Method LocalizationManager.FixRTL_IfNeeded(string)  does RTL fixing if the current language is RTL
NEW: TermsPopup attribute was added to display a string as a popup with the list of terms
FIX: The Plugin Manager window now allows op-out of automatic notification whenever there is a new version.
FIX: Tools tab now shows error messages and warnings
FIX: Corrected compile errors regarding ambiguous calls that happened on some projects/platforms
FIX: Fixed compile error when building for METRO about missing ToTitleCase method in the CultureInfo
FIX: NGUI LanguagePopup component now starts with the saved language instead of the first one in the list
FIX: Chinese Simplified/Traditional are now correctly detected when running on the device
FIX: SetTerm was failing when called on a disabled Object

Thanks to tacticsofttech for its contribution to the Parse terms in Scripts!!

2.5.0
- NEW: Terms can now have separated translations for Touch devices. This allows specifying "tap" instead of "click"
- NEW: Increased performance when browsing the terms list in the Language Source
- NEW: Add a new version to the required Google Service
- NEW: Localize can modify case not only to UPPER and lower but to UpperFirst("This is an example") and to Title ("This Is An Example")
- NEW: Scenes List in the Tools tab can now be collapsed
- FIX: Google Translation was failing for some strings with mixed or title casing e.g. ("Not Enough Rope" was not translating)
- FIX: SetLanguage component Inspector was not showing. Now it displays a dropdown to select the language

2.4.5 
- NEW: Import/Export CSV files now supports changing the separator character (Comma, Semicolon, Tab)
- NEW: The Localization is now initialized when calling HasLanguage to allow changing the language before requesting any Translation
- NEW: The tool to bake the terms into ScriptLocalization.cs now replaces invalid characters by '_'
- NEW: Terms in ScriptLocalization.cs can now be clamped to a maximum length, Terms that clash are enumatated (Examp_1, Examp_2)
- NEW: When creating languages, those with a variant didn't list the base language, now the list includes the base (e.g English)
- NEW: All languages in the Add Language popup show the Language Code for easier identification
- NEW: Not all international language codes are supported by Google Translator. A fallback language is now provided for those.
- FIX: Translating to some languages by using the "Translate" button on the Localize component was failing for some languages
- FIX: When the Language Source had lot of languages, the Terms list was sometimes displayed empty when scrolling

2.4.4
- NEW: Menu: Tools\I2 Localization\About opens a window showing if there is a new version and has shortcuts to useful information
- NEW: Whenever there is a new version the editor automatically alert you. There are options to opt-out or skip a version
- NEW: Clicking the Translate button next to the Term translations will now use the Term name if no other translation is found
- NEW: Language Source Inspector has now better performance showing the list of terms, languages and scenes
- NEW: The list of languages in the Language Source inspector is now expanded to cover the available space
- NEW: LocalizationCallback can now access the static variables CallBackTerm, CallBackSecondaryTerm, MainTranslation, SecondaryTranslation
- FIX: No longer need to call LocalizationManager.UpdateSource and UpdateDictionary before using LocalizationManager.GetTermsList()
- FIX: NGUI example scene had missing references as the example NGUI atlas changed
- FIX: The UpgradeManager was failing on Unity 5 when accessing the BuildTargetGroup.Unknown
- FIX: Importing from CSV and Google Spreadsheets was ignoring the Language Codes and merging those with identical name

2.4.3
- NEW: Localizing UGUI sprites now supports sprites of type "Multiple"
- NEW: Menu Options to disable/enable auto plugins detection  (menu: Tools/I2 Localization/Enable Plugins/...)
- FIX: Using Localize.SetTerm(term) on the Start or Awake functions will not get reverted to the default value
- FIX: Checks for when Localizing prefabs but the referenced objects are not found.
- FIX: Added support for Unity 5.0.0f3

2.4.2
- NEW: Added an optional bool to allow fixing for RTL when using translation = ScriptLocalization.Get(xxx, true)
- FIX: Realtime translation was failing on some mobile devices
- FIX: Fix error when localizing not empty or non existing terms(this caused Sprites and other Secondary Translations to fail)
- FIX: Terms are now saved after importing them from google

2.4.1 f2
- NEW: ScriptLocalization.cs and I2Language.prefab are now autogenerated so they will not override existing localizations
- NEW: The plugin now detects when using TextMeshPro or TextMeshPro beta, and adds a conditional TMProBeta if the beta is used
- NEW: Local Spreadsheets can now be saved as CSV or CSV renamed as TXT (this last avoids the Unity crash when on Mac)
- FIX: OnLocalize Callbacks were not called inside the IDE. 
- FIX: Errors when compiling to WebPlayer
 
2.4.0
- NEW: Added support for multiple Global Sources. By default is only "I2Languages" but you can add any other in LocalizationManager.GlobalSources
- NEW: Dynamic Translation work now in the game by using Google Translator to translate chat messages and other dynamic texts.
- NEW: Localize component will detect automatically which sources contain the translation for its term
- NEW: Tool "Find No Localized Labels" can now filter which labels to include/exclude
- NEW: There is now a button to unlink the Google Spreadsheet Key
- NEW: Added quick links in the Source and Localize inspector to access the Forum, Tutorials and Documentation
- NEW: Google WebService now has a version number and the plugin will detect if that version is supported and ask you to upgrade
- FIX: Compile errors that prevented compiling for W8P and METRO
- FIX: Adding a Localize component at run time will now initialize its variables correctly
- FIX: Renamed some Example scripts to avoid conflicts. Also added them to the I2.Loc namespace
- FIX: When secondary translation is not set, it will take the value from the object (e.g. Font Name, Atlas, etc)
- FIX: Tool "Find No Localized Labels" now work with TextMesh, TextMeshPro, UI.Text, etc.
- FIX: Avoided creating multiple PlayerPrefs entries for the same language Source (LastGoogleUpdateXXXX)
- FIX: No longer is possible to rename/create a term if the new term already exists.
- DEL: The console message saying that no terms were found in the scene is now removed and only shown as part of the inspector

2.3.2
- NEW: import CSV fill autodetect if the Type or Desc columns are missing
- NEW: SpriteCollection shows now in the Type List in the editor for TK2D
- NEW: Added callback for when a language source is autodated from Google (Event_OnSourceUpdateFromGoogle)
- NEW: Increased translation lookup speed by using a fast string comparer in the dictionary
- NEW: Added a toggle in the Language Source to allow lookup the term with Case Insensitive comparison
- FIX: Terms list on the source will not longer cut off visible elements
- FIX: LoalizationManager.GetLanguageFromCode was returing the code instead of the language name
- FIX: Localization is now skipped if the Main and Secondary translations aren't changed

2.3.1
- NEW: Support for TextMeshPro UGUI objects
- NEW: Auto Update from google spreadsheets can now be set to ALWAYS, NEVER, DAILY, WEEKLY, MONTHLY
- NEW: Added functions to get/change the language based on the language code
- NEW: Added functions TryGetTranslation to both LocalizationManager and LanguageSource
- NEW: Language is now only remembered if the user changes it manually and ruled by the device language otherwise. 
- FIX: The plugin is now Initialized automatically when requesting a translation or language code
- FIX: Changing the term category was not displaying correctly until the project was reopened
- FIX: Exporting to google as "Add New" was changing the order of languages
- FIX: Compile errors that prevented deploying to Windows Store
- FIX: The editor was not allowing to add language regions (e.g. English (US), English (CA))
- FIX: Auto Update Google dropdown box was not rendering correctly on all screen sizes

2.3.0
- NEW: Google Synchronization now uses a Web Service to avoid using the username/password
- NEW: When playing (even on a device) the game will download the latest changes to the spreadsheet
- NEW: Added support for both the "Classic" and new Google Spreadsheets
- NEW: Button to create a new spreadsheet
- NEW: Importing/Exporting to Google is now an Async operation that doesn't lock the editor and can be canceled
- NEW: Next to the Google Spreadsheet Key there is now a button to open it in the browser
- NEW: Google Import/Export tab will be the default (instead of local file) whenever a spreadsheet Key is set
- NEW: Import/Export can now be set to Replace all Terms, Merge or only add the New Terms
- NEW: A warning is now shown when using a LanguageSource other than the recommended I2Languages.prefab
- NEW: Menu option to open the Global Source I2Languages.prefab (Menu : Tools/I2/Localization/Open GLobal Source)
- NEW: Google Spreadsheet now has a new format, where the description and term type are defined as notes
- FIX: When switching terms or tabs the textfields will not longer keep the previous text
- DEL: Removed support for the old NGUI TextAssets as NGUI has moved into CSV files
- DEL: Removed Google API libraries dependencies
- DEL: The spreadsheet Key is no longer needed. The web service will get all the keys and allow you to select

2.2.1 b1
- NEW: Improved Language Recognition. It will now fallback to any region of the same Language
- NEW: Right To Left text rendering example scene
- NEW: DFGUI labels and buttons will be able to localize dynamic and bitmap fonts
- NEW: UI.RawImage Localization
- FIX: UI.Sprite Localization was not loading the Sprite from the Resource folder
- FIX: Up and Down arrows on the Languages list was not ordering the languages
- FIX: Detection of Unity UI (updated to 4.6)
- FIX: Unity UI example scene now uses the 4.6 UI classes
- FIX: Right To Left languages was not detected because the language code wasn't being applied

2.2.0
- NEW: Added support for TextMeshPro
- NEW: Terms can now have category and subcategories (e.g. Tutorials/Tutorial1/Startup/Title)
- FIX: NGUI is now detected by looking for the NGUIDebug class instead of UIPanel

2.1.0 f1
- NEW: After importing CSV or Google Spreadsheets, the category filter is set to show every term
- NEW: Terms list is now fully expanded on the Language Source
- NEW: Localize Component now has an Option to convert to (Upper, Lower, DontModify) the translations
- FIX: Validations for when importing Spreadsheets with empty columns/languages

2.1.0 b3
- NEW: The plugin is now compatible with Unity 5 (up to alpha 11)
- NEW: Register a function in the event LocalizationManager.OnLocalizeEvent to get called when the language changes
- FIX: Updated the example scenes to use the new Language Sources
- FIX: Terms are now saved correctly after importing a CSV or a Google Spreadsheet
- FIX: Allowed methods with one argument to be used as Localization CallBacks
- FIX: SelectNoLocalizedLabels was running every frame after executed
- DEL: Removed button to select CSV file. Now the Import and Export buttons display the open/save dialog

2.1.0 b2
- FIX: W8P and Metro compatibility
- FIX: Compiler warnings

2.1.0 b1
- NEW: Terms database is now saved within the LanguageSource and not a separated Language Files
- NEW: The selected language is now saved to the PlayerPrefs into "I2 Language"
- NEW: On the Localize Component, creating a key shows a list of terms as you type and their usage
- NEW: On the Localize Component, when changing the translation of a term shows a preview in the target (label/etc)
- NEW: When selecting a Term in the Localize Component, the list can be filtered with the Create Term string
- NEW: On the Localize Component, the Terms List is now sorted Case Insensitive
- NEW: The auto-enable Plugins will set the Script Define Symbols for ALL platforms (IOS,Android,Web,etc)
- NEW: In the Localize Component, the textField thats used for create a key now has a clear button to easy editing
- NEW: If a term is not found when localizing an object the object is left untouched (Previously labels got empty)
- NEW: There is now a button in the Localize Component to quickly rename a Term in the current scene
- DEL: Removing the Editor Databased used to cache the Language Files because all the info is now in the LanguageSource
- FIX: Selecting the CSV file to export will now allow you to create a new file
- FIX: Added a message to explain when exporting fails because the file is Read-Only or its open in other program
- FIX: When exporting to a file inside the project, the "Assets/" section was been skipped
- FIX: Import and Export CSV files now also works on when the editor is set to Web Player
- FIX: Exporting CSV now uses UTF8 encoding to keep special characters
- FIX: The "Open Source" button on the Localize Component now selects the Primary or Secondary term based on the selected tab
- FIX: Terms are now trimmed because spaces at the end/beggining can lead to confusions
- FIX: The list of terms was not showing correctly when selecting MISSING but unselecting USED

2.0.3 f1
- NEW: Support for localizing 2D-ToolKit (TextMeshes and Sprites)

2.0.3 b2
- NEW: When more than one localization type is available, the plugin allows you to select which component to localize
- FIX: When localizing secondary elements (Atlas, Fonts) the system checks that they still exist to avoid null exceptions
- FIX: Localization of Prefab now have the lowest priority to easy localizing labels/sprites with childs

2.0.3 b1
- NEW: The plugin will now check and enable by default all Plugins included in the project (NGUI,DFGUI,UGUI)
- NEW: Global Localization Source (I2Languages) its now empty by default to make it easy to start a new project
- FIX: Moved the Terms used in each example scene to a new Language Source inside each scene
- DEL: Removed Resources.UnloadAsset when changing the localization to avoid unloading referenced assets

2.0.2 f1
- NEW: UIFonts fonts can now be localized on NGUI
- FIX: Some example scenes were corrupted
- FIX: Modified the plugin to be compatible with Unity5

2.0.1 f1
- NEW: When an object is set as a translation, the object is also added automatically to the Reference array
- FIX: Importing from Google Spreadsheets will not longer generate 'Description' as a language
- FIX: The editor will show a message if exporting to Google fails
- FIX: The variable is IsLeft2Right was renamed as IsRight2Left to match its behavior
- FIX: Importing Google Spreadsheets no longer duplicate the languages
- FIX: Importing CSV was skipping some languages and not parsing terms after import
- FIX: Converted encoded translations into its ASCII characters ("Il s\x26#39;agit" -> "Il s'agit")
- FIX: Terms Section in the Localize custom editor can be collapsed
- FIX: Localize custom editor becomes more compact and easy to read when several sections are collapsed
- FIX: Expanded Terms in the Terms Tab of the LanguageSource will display an Arrow to make evident that they can be collapsed
- FIX: Terms description is now collapsed automatically when another term is selected
- FIX: The spreadsheet was been opened in the browser even if the Open Spreadsheet after Export flag was disabled

2.0.0 a2
- NEW: Support for languagges using Right To Left (RTL) with correct rendering for Arabic languages.
- NEW: Added a toggle on the Localize component to allow discarding RTL processing for selected objects.
- NEW: Languages can now have a Language code to allow for Language Regions (e.g. English Canada vs English United States)
- NEW: Automatic Translation using Google Services will use the language code instead of the Language Name
- NEW: CSV and Google Spreadsheets will save the language code if needed
- FIX: When adding a language to a source the editor will not switch to the Terms tab. That to allows adding several languages at once.
- FIX: Menu options was moved from "Menu > Assets > I2 Localization" to "Tools > I2 Localization"
- FIX: Localization Manager will not allow changing to a language that doesn't exist

2.0.0 a1
- NEW: Support for Daikon Forge GUI components
- NEW: Support for uGUI as of the Unity 4.6 beta 2 (this is only available for users in the beta test group)
- NEW: Terms can now have a type (Text, Object, Audio, Font, Sprite)
- NEW: Terms can be set to generate the ScriptLocalization.cs for Compile-Time-Checking of used Terms.
- FIX: Changed the Terms preview based on the Term Type
- FIX: Language Sources can now be in the Resources folder, the scene or bundled
- FIX: Component Localize allows to change the target for localizing more than one component in one GameObject

1.8.0
- NEW: Callbacks can be setup on the editor for correct concatenation according to the language 
- NEW: Event system for callbacks with reflection
- NEW: Moved all localization calls into events for localizing more types of components without much code change
- FIX: Moved NGUI and UnityStandard localization code into separated files to minimize dependencies

1.7.0
- NEW: Localize component has now Primary and Secondary Terms
- NEW: Secondary term allows localizing Fonts on Labels
- NEW: Secondary term allows localizing Atlas on Sprites
- NEW: Support for localizing Prefabs
- NEW: Support for localizing GUITexture

1.6.0
- NEW: Added separated components to localize labels and sprites to remove the dependency with the NGUI localization
- NEW: Support for localizing Audio Clips
- NEW: Support for localizing GUIText
- NEW: Support for localizing TextMesh

1.4.0
- NEW: The filter on the Terms list can now have multiple values (e.g. "Tuto;Beg" will show only the terms containing "Tuto" or "Beg"
- NEW: Added References to the UILocalize component to be able of store not only text but also objects
- FIX: UILocalize will now show the Localization source it references

1.3.0
- NEW: Languages can now be moved up and down to organize them
- NEW: Allowed to filter by category on the Terms list
- FIX: First language in the list becomes now the starting/default language

1.2.0
- NEW: Merged Import and Export tabs to allow for external data sources that could be synchronized
- NEW: Ability to categorize Terms to improve organization (e.g. Tutorial, Main, Game Screen, etc)
- NEW: Each term category exports into a separated sheet when linking to Google Spreadsheets
- NEW: Parsing scenes for changing the category on selected terms

1.0.2
- FIX: Improved performance on the inspector by removing unneeded Layout functions
- FIX: General Code Cleanup

1.0.1 
- NEW: Custom Editors now allow Undo the changes on the keys and startingLanguage
- FIX: Removed testing Log calls

1.0.0 f2
- FIX: Parsing scenes was executed several times in a row or not at all.
- FIX: Importing CSV will now parse the current scene to show Key Usages
- FIX: A message is shown when Selecting All No Localized labels in scene, if there are none
- FIX: Clicking on the usage number of unused keys will not try to select them
- FIX: Merging Keys will save scenes to avoid loosing changes
- FIX: Sometimes exporting without saving made changes to be lost. Now it automatically saves data if needed.


1.0.0 f1
- NEW: The language TextAsset will be shown in the Language list instead of just the name. That allows finding the asset, moving it to another folder, etc
- NEW: Languages can now be also added by dragging a TextAsset into the Add Language bars.
- NEW: Keys that are are missing the translation in any of the languages are highlighted in the Keys List by making them Italic and Darker
- DEL: Removed button Update NGUI in the Key list. All data will be saved automatically when the inspector view changes to another object or the editor is closed
- FIX: Filter for list of keys now is case insensitive.
- FIX: Auto opening google Spreadsheet after export was opening two web pages.
- FIX: Deleting a language will not only unlink the TextAsset from NGUI but will also delete the text file.
- FIX: If a TextAsset is manually deleted, but NGUI still keeps a reference in the language list, that language is now skipped
- FIX: Removed compile warnings when in WebPlayer platform
- FIX: Removed exception when adding keys before creating a language
- FIX: Adding multiple keys to NGUI was only adding the first one and returning an exception

1.0.0 b2
- NEW: Added a TextField to filter the list of keys.
- NEW: Option to auto open the Google Spreadsheet doc after exporting.
- NEW: Added a centralized Error reporting.
- NEW: Option to save or not the google password.
- NEW: Added a menu option to quickly access the help.  (Help\I2 Localization For NGUI).
- NEW: Key list show a warning icon on the keys that are used in the scenes but are not in the NGUI files.
- FIX: An error will show when contacting Google Translation on the WebPlayer Platform as its not yet supported.
- FIX: Google public spreadsheet Key is now remembered when the editor opens.

1.0.0 b1
- NEW: First Version including core features.
