# XamarinLocalizationSync
synchronize multilanguage ressource-strings between .net resx-files and android ressource-strings.xml

it's a simple tool and ios will follow next

# BACKUP YOUR FILES BEFORE FIRST SYNC !!!

so for those who like or need to have multilanguage-strings accessable anywhere usage and requirements:
```
  android folder structure as default:
  ..ressources\values\strings.xml
  ..ressources\values-de\strings.xml

  .net resx-files in same folder
  ..localize\localize.resx
  ..localize\localize-de.resx
```
the names for your files and folders doesn't mind alsong you use same language-postfix

## usage:
```
  XamarinLocalizationSync /xmlPath=d:\Projects\TestApp\TestApp.Android\Resources\values\strings.xml /resxPath=d:\Projects\TestApp\TestApp.Core\localize\localize.resx /syncToXml /syncToResx
```

## what happenes:
-parameter /syncToXml means all items from your resx-file will be copied to android-xml, existing values will be overwritten, removed items will also be removed in xml as they have ever been synced before

-parameter /syncToResx means all string's from android xml will be copied to resx-file, existing values will be overwritten

as I use resx-files als main-storage that should win in case of conflicts syncToXml will be executed first if you set both parameters so if you prefer the other way around just execute it twice in the mode you prefer

## output:
```
  my name is XamarinLocalizationSync
  Param:xmlpath : d:\Projects\TestApp\TestApp.Android\Resources\values\strings.xml
  Param:resxpath : d:\Projects\TestApp\TestApp.Core\localize\localize.resx
  Param:synctoresx is set
  Param:synctoxml is set

  start syncToXml
  values\strings.xml => localize.resx
  total 320       updated 320     new 0   deleted 0

  values-de\strings.xml => localize.de.resx
  total 316       updated 316     new 0   deleted 0

  start syncToResx
  values\strings.xml => localize.resx
  total 320       updated 320     new 0

  values-de\strings.xml => localize.de.resx
  total 316       updated 316     new 0

  all done
```
as you see all files in other languages are detected and synced  
in parameters you only have to add the default-language-file  
and as you mey also see in this sample 4 items are not translated to german  
attention:  
the sync only happenes when both files exist so if you add a new language in you have to create the "other" file before sync  

## howto android string-arrays?
the way I prefer is create a new file f.e. string-arrays.xml in main-values folder and add content like this:
```
    <string-array name="test_array" translatable="false">
        <item>@string/viewtype_Timeline</item>
        <item>@string/viewtype_DayView</item>
        <item>@string/viewtype_WeekView</item>
        <item>@string/viewtype_WorkWeek</item>
        <item>@string/viewtype_MonthView</item>
    </string-array>
```
so you only have to manage you arrays once and you also have easy access to the single items
