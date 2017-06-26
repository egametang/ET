// **************************************************
// This file created by Brett Bibby (c) 2010-2013
// You may freely use and modify this file as you see fit
// You may not sell it
//**************************************************
// hidden object game exporter
//$.writeln("=== Starting Debugging Session ===");

// enable double clicking from the Macintosh Finder or the Windows Explorer
#target photoshop

// debug level: 0-2 (0:disable, 1:break on error, 2:break at beginning)
// $.level = 0;
// debugger; // launch debugger on next line

var sceneData;
var sourcePsd;
var duppedPsd;
var destinationFolder;
var uuid;
var sourcePsdName;
var slicePaddingArr = new Array(0,0,0,0)
var sliceOriArr = new Array(0,0,0,0)

main();

function main(){
    // got a valid document?
    if (app.documents.length <= 0)
    {
        if (app.playbackDisplayDialogs != DialogModes.NO)
        {
            alert("You must have a document open to export!");
        }
        // quit, returning 'cancel' makes the actions palette not record our script
        return 'cancel';
    }

    // ask for where the exported files should go
    destinationFolder = Folder.selectDialog("Choose the destination for export.");
    if (!destinationFolder)
    {
        return;
    }

    // cache useful variables
    uuid = 1;
    sourcePsdName = app.activeDocument.name;
    var layerCount = app.documents[sourcePsdName].layers.length;
    var layerSetsCount = app.documents[sourcePsdName].layerSets.length;

    if ((layerCount <= 1) && (layerSetsCount <= 0))
    {
        if (app.playbackDisplayDialogs != DialogModes.NO)
        {
            alert("You need a document with multiple layers to export!");
            // quit, returning 'cancel' makes the actions palette not record our script
            return 'cancel';
        }
    }

    // setup the units in case it isn't pixels
    var savedRulerUnits = app.preferences.rulerUnits;
    var savedTypeUnits = app.preferences.typeUnits;
    app.preferences.rulerUnits = Units.PIXELS;
    app.preferences.typeUnits = TypeUnits.PIXELS;

    // duplicate document so we can extract everythng we need
    duppedPsd = app.activeDocument.duplicate();
    duppedPsd.activeLayer = duppedPsd.layers[duppedPsd.layers.length - 1];

    hideAllLayers(duppedPsd);

    // export layers
    sceneData = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
    sceneData += "<PSDUI>";
    
    sceneData += "\n<psdSize>";
    sceneData += "<width>" + duppedPsd.width.value + "</width>";
    sceneData += "<height>" + duppedPsd.height.value+ "</height>";
    sceneData += "</psdSize>";
    
    sceneData += "\n<layers>";
    exportAllLayers(duppedPsd);
    sceneData += "</layers>";

    sceneData += "\n</PSDUI>";
    $.writeln(sceneData);

    duppedPsd.close(SaveOptions.DONOTSAVECHANGES);

    // create export
    var sceneFile = new File(destinationFolder + "/" + destinationFolder.name + ".xml");
    sceneFile.encoding = "utf-8";   //写文件时指定编码，不然中文会出现乱码
    sceneFile.open('w');
    sceneFile.writeln(sceneData);
    sceneFile.close();

    app.preferences.rulerUnits = savedRulerUnits;
    app.preferences.typeUnits = savedTypeUnits;
}

function exportAllLayers(obj)
{
    if  (typeof(obj) == "undefined"){
        return;
    }

    if (typeof(obj.layers) != "undefined" && obj.layers.length>0) {

        for (var i = obj.layers.length - 1; 0 <= i; i--)
        {
            exportLayer(obj.layers[i])
        }
        
    }
    else{
        exportLayer(obj)
    };
}

function exportLayer(obj)
{
    if  (typeof(obj) == "undefined"){
        return;
    }

    if (obj.typename == "LayerSet") {
            exportLayerSet(obj);
    }
    else if  (obj.typename = "ArtLayer"){
        exportArtLayer(obj);
    }
}

function exportLayerSet(_layer)
{
    if (typeof(_layer.layers) == "undefined" || _layer.layers.length<=0 ){
        return
    }
    if (_layer.name.search("@ScrollView") >= 0)
    {
      exportScrollView(_layer);
  }
  else if (_layer.name.search("@Grid") >= 0)
  {
      exportGrid(_layer);
  }
  else if (_layer.name.search("@Button") >= 0)
  {
      exportButton(_layer);
  }
  else if (_layer.name.search("@Toggle") >= 0)
  {
      exportToggle(_layer);
  }
  else if (_layer.name.search("@Panel") >= 0)
  {
      exportPanel(_layer);
  }
  else if (_layer.name.search("@Slider")>=0) {
      exportSlider(_layer);
  }
  else if (_layer.name.search("@Group")>=0) {
      exportGroup(_layer);
  }
  else if (_layer.name.search("@InputField") >=0) {
      exportInputField(_layer);
  }
  else if (_layer.name.search("@Scrollbar") >=0) {
      exportScrollBar(_layer);
  }else if (_layer.name.search("@LE") >=0) {//增加布局元素导出
        exportLayoutElement(_layer);
    }
  else
  {
    sceneData += "<Layer>";
    sceneData += "<type>Normal</type>";
    sceneData += "<name>" + _layer.name + "</name>";    
    sceneData += "<layers>";
    exportAllLayers(_layer)
    sceneData += "</layers>";
    sceneData += "</Layer>";
  }
}

function exportLayoutElement(obj)
{
    sceneData += "<Layer>";
    sceneData += "<type>LayoutElement</type>";
    var itemName = obj.name.substring(0, obj.name.search("@"));
    sceneData += "<name>" + itemName + "</name>";

    sceneData += "<layers>";
    exportAllLayers(obj);

    // sceneData += "<images>";
    // for (var j = obj.artLayers.length - 1; 0 <= j; j--)
    // {
    //     exportArtLayer(obj.artLayers[j]);
    // }
    // sceneData += "</images>";
    sceneData += "</layers>";
    
    obj.visible = true;
    showAllLayers(obj); 
    
    var recSize = getLayerRec(duppedPsd.duplicate());

    sceneData += "<position>";
    sceneData += "<x>" + recSize.x + "</x>";
    sceneData += "<y>" + recSize.y + "</y>";
    sceneData += "</position>";

    sceneData += "<size>";
    sceneData += "<width>" + recSize.width + "</width>";
    sceneData += "<height>" + recSize.height + "</height>";
    sceneData += "</size>";

    hideAllLayers(obj);
    
    sceneData += "</Layer>";
}

function exportScrollView(obj)
{
    var itemName = obj.name.substring(0, obj.name.search("@"));
    sceneData += ("<Layer>\n<type>ScrollView</type>\n<name>" + itemName + "</name>\n");
    sceneData += ("<layers>\n");
    exportAllLayers(obj);
    sceneData += ("</layers>");

    var params = obj.name.split(":");

    if (params.length > 2)
    {
        alert(obj.name + "-------Layer's name is illegal------------");
    }

    var recSize;
    if (obj.layers[obj.layers.length - 1].name.search("@Size") < 0)
    {
        alert("Bottom layer's name doesn't contain '@Size'");
    }
    else
    {
        obj.layers[obj.layers.length - 1].visible = true;

        recSize = getLayerRec(duppedPsd.duplicate());

        sceneData += "<position>";
        sceneData += "<x>" + recSize.x + "</x>";
        sceneData += "<y>" + recSize.y + "</y>";
        sceneData += "</position>";

        sceneData += "<size>";
        sceneData += "<width>" + recSize.width + "</width>";
        sceneData += "<height>" + recSize.height + "</height>";
        sceneData += "</size>";

        obj.layers[obj.layers.length - 1].visible = false;
    }
    
    //以下计算padding和spacing
    obj.layers[0].visible = true;
    showAllLayers(obj.layers[0]);                           //子图层组已经在上面导出过，要再次计算size需先将其显示
    var rec0 = getLayerRec(duppedPsd.duplicate());
    hideAllLayers(obj.layers[0]);
    obj.layers[0].visible = false;
    
    obj.layers[1].visible = true;
    showAllLayers(obj.layers[1]);
    var rec1 = getLayerRec(duppedPsd.duplicate());
    hideAllLayers(obj.layers[0]);
    obj.layers[1].visible = false;
    
    var spacing;
    var paddingx;
    var paddingy;
    if(params[1].search("H") >= 0)          //水平间距
    {
        spacing = rec1.x - rec0.x - rec0.width;
        paddingx =  rec0.x - (recSize.x - recSize.width / 2) -  rec0.width / 2;                                      //x方向边距，默认左右相等
        paddingy = (recSize.height - rec0.height) / 2 ;                                                          //暂时只考虑上下边距相等             
        //paddingy = recSize.height / 2 - rec0.height / 2 - (rec0.y - recSize.y);                                                                   //上边距
        //paddingy2 = recSize.height - rec0.height - paddingy;                      //下边距
    }
    else                                                //垂直间距
    {
        spacing = rec0.y - rec1.y - rec0.height;
        paddingx =  (recSize.width - rec0.width) / 2 ; 
        paddingy = (recSize.y + recSize.height / 2)  - rec0.y -  rec0.height / 2;     
    }                                    

    sceneData += "<arguments>";
    sceneData += "<string>" + params[1] + "</string>";     //滑动方向
    sceneData += "<string>" + spacing + "</string>";   
    sceneData += "<string>" + Math.floor (paddingx) + "</string>";  
    sceneData += "<string>" + Math.floor (paddingy) + "</string>";  
    sceneData += "</arguments>";

    sceneData += "</Layer>";
}

function setLayerSizeAndPos(layer)
{
    layer.visible = true;

    var recSize = getLayerRec(duppedPsd.duplicate());

    sceneData += "<position>";
    sceneData += "<x>" + recSize.x + "</x>";
    sceneData += "<y>" + recSize.y + "</y>";
    sceneData += "</position>";

    sceneData += "<size>";
    sceneData += "<width>" + recSize.width + "</width>";
    sceneData += "<height>" + recSize.height + "</height>";
    sceneData += "</size>";

    layer.visible = false;
}

function exportGrid(obj)
{
    var itemName = obj.name.substring(0, obj.name.search("@"));
    sceneData += ("<Layer>\n<type>Grid</type>\n<name>" + itemName + "</name>\n");
    sceneData += ("<layers>\n");
    exportAllLayers(obj);
    sceneData += ("</layers>");

    var params = obj.name.split(":");

    if (params.length != 3)
    {
        alert("Layer's name is illegal");
    }

    var recSize;
    if (obj.layers[obj.layers.length - 1].name.search("@Size") < 0)
    {
        alert("Bottom layer's name doesn't contain '@Size'");
    }
    else
    {  
        setLayerSizeAndPos(obj.layers[obj.layers.length - 1]);
    }

    var totalContentCount = obj.layers.length - 1;  
    
    obj.layers[0].visible = true;
    showAllLayers(obj.layers[0]);                           //子图层组已经在上面导出过，要再次计算size需先将其显示
    var rec0 = getLayerRec(duppedPsd.duplicate());
    hideAllLayers(obj.layers[0]);
    obj.layers[0].visible = false;
    
    var renderHorizontalGap = params[2] > 1 ? (recSize.width - rec0.width * params[2])/(params[2] - 1) : 0;
    var renderVerticalGap = params[1] > 1 ? (recSize.height - rec0.height * params[1])/(params[1] - 1) : 0;

    sceneData += "<arguments>";
    sceneData += "<string>" + params[1] + "</string>";   //行数
    sceneData += "<string>" + params[2] + "</string>";   //列数
    sceneData += "<string>" + rec0.width + "</string>";   //render width
    sceneData += "<string>" + rec0.height + "</string>";   //render height
    sceneData += "<string>" + Math.floor(renderHorizontalGap) + "</string>"; //水平间距
    sceneData += "<string>" + Math.floor(renderVerticalGap) + "</string>"; //垂直间距
    sceneData += "</arguments>";

    sceneData += "</Layer>";
}

function exportGroup(obj)
{
    var itemName = obj.name.substring(0, obj.name.search("@"));
    sceneData += ("<Layer>\n<type>Group</type>\n<name>" + itemName + "</name>\n");

    exportAllLayers(obj);

    var params = obj.name.split(":");

    if (params.length != 3 )
    {
        alert(obj.name + "-------Layer's name not equals 2------------");
    }

    var recSize;
    if (obj.layers[obj.layers.length - 1].name.search("@Size") < 0)
    {
        alert("Bottom layer's name doesn't contain '@Size'");
    }
    else
    {
        obj.layers[obj.layers.length - 1].visible = true;

        recSize = getLayerRec(duppedPsd.duplicate());

        sceneData += "<position>";
        sceneData += "<x>" + recSize.x + "</x>";
        sceneData += "<y>" + recSize.y + "</y>";
        sceneData += "</position>";

        sceneData += "<size>";
        sceneData += "<width>" + recSize.width + "</width>";
        sceneData += "<height>" + recSize.height + "</height>";
        sceneData += "</size>";

        obj.layers[obj.layers.length - 1].visible = false;
    }

    sceneData += "<arguments>";
    sceneData += "<string>" + params[1] + "</string>";   //方向
    sceneData += "<string>" + params[2] + "</string>";   //span
    sceneData += "</arguments>";

    sceneData += "</Layer>";
}

function exportInputField(obj)
{
    var itemName = obj.name.substring(0, obj.name.search("@"));
    sceneData += ("<Layer>\n<type>InputField</type>\n<name>" + itemName + "</name>\n");
    sceneData += "<layers>";

    // sceneData += "<images>\n";

    for (var i = obj.layers.length - 1; 0 <= i; i--)
    {
        exportArtLayer(obj.layers[i]);
    }

    sceneData += "</layers>";
    // sceneData += "\n</images>\n</Layer>";
    sceneData += "\n</Layer>";
}

function exportButton(obj)
{
    var itemName = obj.name.substring(0, obj.name.search("@"));
    sceneData += ("<Layer>\n<type>Button</type>\n<name>" + itemName + "</name>\n");
    sceneData += "<layers>";

    // sceneData += "<images>\n";

    for (var i = obj.layers.length - 1; 0 <= i; i--)
    {
        exportArtLayer(obj.layers[i]);
    }
    sceneData += "</layers>";
    // sceneData += "\n</images>\n</Layer>";
    sceneData += "\n</Layer>";
}

function exportToggle(obj)
{
    var itemName = obj.name.substring(0, obj.name.search("@"));
    sceneData += ("<Layer>\n<type>Toggle</type>\n<name>" + itemName + "</name>\n");
    sceneData += "<layers>";

    // sceneData += "<images>\n";

    for (var i = obj.layers.length - 1; 0 <= i; i--)
    {
        exportArtLayer(obj.layers[i]);
    }

    sceneData += "</layers>";
    // sceneData += "\n</images>\n</Layer>";
    sceneData += "\n</Layer>";
}

function exportSlider(obj)
{
    var itemName = obj.name.substring(0, obj.name.search("@"));
    sceneData += ("<Layer>\n<type>Slider</type>\n<name>" + itemName + "</name>\n");

    var params = obj.name.split(":");

    if (params.length != 2)
    {
        alert(obj.name + "-------Layer's name is not 1 argument------------");
    }
    
    var recSize;
    if (obj.layers[obj.layers.length - 1].name.search("@Size") < 0)
    {
        alert("Bottom layer's name doesn't contain '@Size'");
    }
    else
    {
        setLayerSizeAndPos(obj.layers[obj.layers.length - 1]);
    }

    sceneData += "<arguments>";
    sceneData += "<string>" + params[1] + "</string>"; //滑动方向
    sceneData += "</arguments>";
    
    // sceneData += "<images>\n";
    sceneData += "<layers>";

    for (var i = obj.layers.length - 1; 0 <= i; i--)
    {
        exportArtLayer(obj.layers[i]);
    }
    sceneData += "</layers>";

    // sceneData += "\n</images>\n</Layer>";
    sceneData += "\n</Layer>";
}

function exportScrollBar(obj)
{
    var itemName = obj.name.substring(0, obj.name.search("@"));
    sceneData += ("<Layer>\n<type>ScrollBar</type>\n<name>" + itemName + "</name>\n");

    var params = obj.name.split(":");

    if (params.length != 3)
    {
        alert(obj.name + "-------Layer's name is not 1 argument------------");
    }
    
    sceneData += "<arguments>";
    sceneData += "<string>" + params[1] + "</string>"; //滑动方向
    sceneData += "<string>" + params[2] + "</string>"; //比例
    sceneData += "</arguments>";

    // sceneData += "<images>\n";
    sceneData += "<layers>";

    for (var i = obj.layers.length - 1; 0 <= i; i--)
    {
        exportArtLayer(obj.layers[i]);
    }
    sceneData += "</layers>";

    // sceneData += "\n</images>\n</Layer>";
    sceneData += "\n</Layer>";
}

function exportPanel(obj)
{
    var itemName = obj.name.substring(0, obj.name.search("@"));
    sceneData += ("<Layer>\n<type>Panel</type>\n<name>" + itemName + "</name>\n");

    exportAllLayers(obj);

    // sceneData += "<images>\n";
    sceneData += "<layers>";

    for (var j = obj.artLayers.length - 1; 0 <= j; j--)
    {
        exportArtLayer(obj.artLayers[j]);
    }
    sceneData += "</layers>";

    // sceneData += "\n</images>\n</Layer>";
    sceneData += "\n</Layer>";
}

function exportArtLayer(obj)
{
    if (typeof(obj) == "undefined") {return};
    if (obj.name.search("@Size") >= 0) {return};

    sceneData += "\n<Layer>";
    sceneData += "<type>Normal</type>";
    //sceneData += "<name>" + makeValidFileName(obj.name) + "</name>";
    var validFileName = makeValidFileName(obj.name);
    sceneData += "<name>" + validFileName + "</name>";
    sceneData += "<image>\n";
    // sceneData += "<PSImage>\n";
    if (LayerKind.TEXT == obj.kind)
    {
        exportLabel(obj,validFileName);
    }
    else if (obj.name.search("Texture") >= 0)
    {
        exportTexture(obj,validFileName);
    }
    else
    {
        exportImage(obj,validFileName);
    }
    sceneData += "</image>";
    // sceneData += "</PSImage>";
    sceneData += "\n</Layer>";
}

function exportLabel(obj,validFileName)
{
    //有些文本如标题，按钮，美术用的是其他字体，可能还加了各种样式，需要当做图片切出来使用
    if(obj.name.search("_ArtStatic") >= 0)
    {
        exportImage(obj,validFileName);   
        return;
    }

    //处理静态文本，会对应unity的静态字体
    var StaticText = false;
    if(obj.name.search("_Static") >= 0)
    {
        StaticText = true;
    }

    sceneData += "<imageType>" + "Label" + "</imageType>\n";
    //var validFileName = makeValidFileName(obj.name);
    sceneData += "<name>" + validFileName + "</name>\n";
    obj.visible = true;
    saveScenePng(duppedPsd.duplicate(), validFileName, false);
    obj.visible = false;    
    
    sceneData += "<arguments>";
    sceneData += "<string>" + obj.textItem.color.rgb.hexValue + "</string>";
    
    if(StaticText == true)
    {
        sceneData += "<string>" + obj.textItem.font + "_Static" + "</string>";
    }
    else
    {
        sceneData += "<string>" + obj.textItem.font + "</string>";
    }
    //sceneData += "<string>" + obj.textItem.font + "</string>";
    sceneData += "<string>" + obj.textItem.size.value + "</string>";
    sceneData += "<string>" + obj.textItem.contents + "</string>";
    
    //段落文本带文本框，可以取得对齐方式
    if(obj.textItem.kind == TextType.PARAGRAPHTEXT)
    {
        sceneData += "<string>" + obj.textItem.justification + "</string>";     //加对齐方式
    }
    sceneData += "</arguments>";
}

function exportTexture(obj,validFileName)
{
    //var validFileName = makeValidFileName(obj.name);
    sceneData += "<imageType>" + "Texture" + "</imageType>\n";
    sceneData += "<name>" + validFileName + "</name>\n";
    obj.visible = true;
    saveScenePng(duppedPsd.duplicate(), validFileName, true);
    obj.visible = false;
}

function exportImage(obj,validFileName)
{
    //var validFileName = makeValidFileName(obj.name);
    var oriName = obj.name
    sceneData += "<name>" + validFileName + "</name>\n";

    if (obj.name.search("Common") >= 0)
    {
        sceneData += "<imageSource>" + "Common" + "</imageSource>\n";
    }
    else if(obj.name.search("Global") >= 0)
    {
        sceneData += "<imageSource>" + "Global" + "</imageSource>\n";
    }
    else
    {
        sceneData += "<imageSource>" + "Custom" + "</imageSource>\n";      
    }

  if (oriName.search("_9S") >= 0)
  {
      sceneData += "<imageType>" + "SliceImage" + "</imageType>\n";
      obj.visible = true;
      var _objName = obj.name
      // var newDoc = app.documents.add(duppedPsd.width, duppedPsd.height,duppedPsd.resolution, _objName+"doc",NewDocumentMode.RGB,DocumentFill.TRANSPARENT)
      // app.activeDocument = duppedPsd
      // obj.copy()
      // app.activeDocument = newDoc
      // newDoc.paste()
      //   newDoc.activeLayer.name = _objName
      var recSize = getLayerRec(duppedPsd.duplicate(),true);
        sceneData += "<position>";
        sceneData += "<x>" + recSize.x + "</x>";
        sceneData += "<y>" + recSize.y + "</y>";
        sceneData += "</position>";

        sceneData += "<size>";
        sceneData += "<width>" + recSize.width + "</width>";
        sceneData += "<height>" + recSize.height + "</height>";
        sceneData += "</size>";

      // _9sliceCutImg(newDoc,_objName,validFileName); 
      _9sliceCutImg(duppedPsd.duplicate(),_objName,validFileName); 
      obj.visible = false;
      return;
  }
    else if(oriName.search("LeftHalf") > 0)       //左右对称的图片切左边一半
    {
        sceneData += "<imageType>" + "LeftHalfImage" + "</imageType>\n";
        
        obj.visible = true;
        
        var recSize = getLayerRec(duppedPsd.duplicate());
        sceneData += "<position>";
        sceneData += "<x>" + recSize.x + "</x>";
        sceneData += "<y>" + recSize.y + "</y>";
        sceneData += "</position>";

        sceneData += "<size>";
        sceneData += "<width>" + recSize.width + "</width>";
        sceneData += "<height>" + recSize.height + "</height>";
        sceneData += "</size>";
        
        cutLeftHalf(duppedPsd.duplicate(),validFileName); 
        obj.visible = false;
        return;
    }
    else if(obj.name.search("BottomHalf") > 0)     //上下对称的图片切底部一半
    {
        sceneData += "<imageType>" + "BottomHalfImage" + "</imageType>\n";
        
        obj.visible = true;
        
        //半图要先计算出大小和位置
        var recSize = getLayerRec(duppedPsd.duplicate());
        sceneData += "<position>";
        sceneData += "<x>" + recSize.x + "</x>";
        sceneData += "<y>" + recSize.y + "</y>";
        sceneData += "</position>";

        sceneData += "<size>";
        sceneData += "<width>" + recSize.width + "</width>";
        sceneData += "<height>" + recSize.height + "</height>";
        sceneData += "</size>";
        
        cutBottomHalf(duppedPsd.duplicate(),validFileName); 
        obj.visible = false;
        return;
    }
    else if(obj.name.search("Quarter") > 0)     //上下左右均对称的图片切左下四分之一
    {
        sceneData += "<imageType>" + "QuarterImage" + "</imageType>\n";
        
        obj.visible = true;
        
        var recSize = getLayerRec(duppedPsd.duplicate());
        sceneData += "<position>";
        sceneData += "<x>" + recSize.x + "</x>";
        sceneData += "<y>" + recSize.y + "</y>";
        sceneData += "</position>";

        sceneData += "<size>";
        sceneData += "<width>" + recSize.width + "</width>";
        sceneData += "<height>" + recSize.height + "</height>";
        sceneData += "</size>";
        
        cutQuarter(duppedPsd.duplicate(),validFileName); 
        obj.visible = false;
        return;
    }
    else
    {
        sceneData += "<imageType>" + "Image" + "</imageType>\n";
    }

    obj.visible = true;
    saveScenePng(duppedPsd.duplicate(), validFileName, true);
    obj.visible = false;
          
}

function hideAllLayers(obj)
{
    hideLayerSets(obj);
}

function hideLayerSets(obj)
{
    for (var i = obj.layers.length - 1; 0 <= i; i--)
    {
        if (obj.layers[i].typename == "LayerSet")
        {
            hideLayerSets(obj.layers[i]);
        }
        else
        {
            obj.layers[i].visible = false;
        }
    }
}

//显示图层组及组下所有图层
function showAllLayers(obj)
{
    showLayerSets(obj);
}

function showLayerSets(obj)
{
    for (var i = obj.layers.length - 1; 0 <= i; i--)
    {
        if (obj.layers[i].typename == "LayerSet")
        {
            showLayerSets(obj.layers[i]);
        }
        else
        {
            obj.layers[i].visible = true;
        }
    }
}


function getLayerRec(psd,notMerge)
{
    // we should now have a single art layer if all went well
    if  (!notMerge){
          psd.mergeVisibleLayers();
        }
  
    // figure out where the top-left corner is so it can be exported into the scene file for placement in game
    // capture current size
    var height = psd.height.value;
    var width = psd.width.value;
    var top = psd.height.value;
    var left = psd.width.value;
    // trim off the top and left
    psd.trim(TrimType.TRANSPARENT, true, true, false, false);
    // the difference between original and trimmed is the amount of offset
    top -= psd.height.value;
    left -= psd.width.value;
    // trim the right and bottom
    psd.trim(TrimType.TRANSPARENT);
    // find center
    top += (psd.height.value / 2)
    left += (psd.width.value / 2)
    // unity needs center of image, not top left
    top = -(top - (height / 2));
    left -= (width / 2);

    height = psd.height.value;
    width = psd.width.value;

    psd.close(SaveOptions.DONOTSAVECHANGES);

    return {
        y: top,
        x: left,
        width: width,
        height: height
    };
}

function saveScenePng(psd, fileName, writeToDisk,notMerge)
{
    // we should now have a single art layer if all went well
    if(!notMerge)
    {
        psd.mergeVisibleLayers();
    }
    
    // figure out where the top-left corner is so it can be exported into the scene file for placement in game
    // capture current size
    var height = psd.height.value;
    var width = psd.width.value;
    var top = psd.height.value;
    var left = psd.width.value;
    // trim off the top and left
    psd.trim(TrimType.TRANSPARENT, true, true, false, false);
    // the difference between original and trimmed is the amount of offset
    top -= psd.height.value;
    left -= psd.width.value;
    // trim the right and bottom
    psd.trim(TrimType.TRANSPARENT);
    // find center
    top += (psd.height.value / 2)
    left += (psd.width.value / 2)
    // unity needs center of image, not top left
    top = -(top - (height / 2));
    left -= (width / 2);

    height = psd.height.value;
    width = psd.width.value;

    var rec = {
        y: top,
        x: left,
        width: width,
        height: height
    };

    // save the scene data
    if(!notMerge){
        sceneData += "<position>";
        sceneData += "<x>" + rec.x + "</x>";
        sceneData += "<y>" + rec.y + "</y>";
        sceneData += "</position>";

        sceneData += "<size>";
        sceneData += "<width>" + rec.width + "</width>";
        sceneData += "<height>" + rec.height + "</height>";
        sceneData += "</size>";
    }
    
     if (writeToDisk)
     {
        // save the image
        var pngFile = new File(destinationFolder + "/" + fileName + ".png");
        var pngSaveOptions = new PNGSaveOptions();
        psd.saveAs(pngFile, pngSaveOptions, true, Extension.LOWERCASE);
    }
    psd.close(SaveOptions.DONOTSAVECHANGES);

}

function makeValidFileName(fileName)
{
    var validName = fileName.replace(/^\s+|\s+$/gm, ''); // trim spaces
    //删除九宫格关键字符
    validName = validName.replace(/\s*_9S(\:\d+)+/g,"")
    validName = validName.replace(/[\\\*\/\?:"\|<>]/g, ''); // remove characters not allowed in a file name
    validName = validName.replace(/[ ]/g, '_'); // replace spaces with underscores, since some programs still may have troubles with them

    if (validName.match("Common") || validName.match("Global"))
    {
        validName = validName.substring (0,validName.lastIndexOf ("@"));  //截取@之后的字符串作为图片的名称。
    }
    else if(!sourcePsdName.match("Common") || !sourcePsdName.match("Global"))    // 判断是否为公用的PSD素材文件，如果不是，则自动为图片增加后缀，防止重名。 公用psd文件的图片层不允许重名。
    {
        validName += "_" + uuid++;
    }
    
     $.writeln(validName);
    return validName;
}

/***************************************************************************************************************************************************************************************************************/
//对称的图片处理，切一半
//2017.01.10
//by zs

// 裁切 基于透明像素
function trim(doc){
    doc.trim(TrimType.TRANSPARENT,true,true,true,true);
}

// 裁剪左半部分
function cutLeftHalf(doc,layerName){
    doc.mergeVisibleLayers();
    
    trim(doc);
    var _obj = doc.activeLayer

    var width = doc.width;
    var height = doc.height;
    var side = width / 2;

    var region = Array(Array(0,height),Array(0,0),Array(side,0),Array(side,height));
        
    var selectRect = doc.selection.select(region);
    doc.selection.copy();
    var newStem = doc.paste();
    newStem.name = layerName;

    var deltaX = 0;
    var deltaY = 0;
    if(region[0][0] != 0){
        deltaX = -(width - side*2);
    }
    newStem.translate(deltaX,deltaY);

    _obj.visible = false;
    trim(doc);
    saveScenePng(doc, layerName, true,true);
    // exportHalfImage(doc,"LeftHalf");
}

// 裁剪下半部分
function cutBottomHalf(doc,layerName){
    doc.mergeVisibleLayers();
    
    trim(doc);
    var _obj = doc.activeLayer
    var width = doc.width;
    var height = doc.height;
    var side = height / 2;

    //var region = Array(Array(0,side),Array(0,0),Array(width,0),Array(width,side));
    var region = Array(Array(0,height),Array(0,side),Array(width,side),Array(width,height));
        
    var selectRect = doc.selection.select(region);
    doc.selection.copy();
    var newStem = doc.paste();
    newStem.name = layerName;

    var deltaX = 0;
    var deltaY = 0;
    if (region[0][1] != side){
        deltaY = -(height - side*2);
    }
    newStem.translate(deltaX,deltaY);

    _obj.visible = false;

    trim(doc);
    saveScenePng(doc, layerName, true,true);
    //exportHalfImage(doc,"UpHalf");
}

// 裁剪左下四分之一
function cutQuarter(doc,layerName){
    doc.mergeVisibleLayers();
    
    trim(doc);
    var _obj = doc.activeLayer
    var width = doc.width;
    var height = doc.height;
    var side = height / 2;

    var region = Array(Array(0,height),Array(0,height / 2),Array(width / 2,height / 2),Array(width / 2,height));
        
    var selectRect = doc.selection.select(region);
    doc.selection.copy();
    var newStem = doc.paste();
    newStem.name = layerName;

    var deltaX = 0;
    var deltaY = 0;
    if (region[0][1] != side){
        deltaY = -(height - side*2);
    }
    newStem.translate(deltaX,deltaY);

    _obj.visible = false;

    trim(doc);
    saveScenePng(doc, layerName, true,true);
}

function exportHalfImage(psd,halfType)
{
    hideAllLayers(psd);
    
    var layerName  = "";
     for (var i = psd.layers.length - 1; 0 <= i; i--)
     {
         layerName = psd.layers[i].name;
         if(layerName.match(halfType))
         {
             psd.layers[i].visible = true;
             saveScenePng(psd, layerName, true,true);
         }
     }
}


/***************************************************************************************************************************************************************************************************************/
//九宫格切图
//2017.01.13
//by HuangLang

function _9sliceCutImg(doc,layerName,vaildName){
    // 创建图层组
    var _obj = doc.activeLayer
    var stemGroup = doc.layerSets.add();
    stemGroup.name = layerName
    // _obj.move(stemGroup,ElementPlacement.PLACEATEND)
    doc.mergeVisibleLayers();
  trim(doc);
   var width = doc.width;
   var height = doc.height;
    var re = /\s*_9S(\:\d+)+/g;
    var getStr = ""
    var result = layerName.match(re)
    if (result) {
        getStr = result[0]
    }else{
        alert("图层名为："+layerName+"的九宫格格式不对！应为_9S:XX或:XX:XX:XX:XX");
        return;
    }

   var  nums = getStr.split(":")
   if ( nums.length == 2) {
      for(var j = 0;j<slicePaddingArr.length;j++)
      {
        sliceOriArr[j] = parseInt(nums[1])
         slicePaddingArr[j] = parseInt(nums[1])
         }
     }
     else if ( nums.length == 5) 
     {
      for(var j = 0;j<slicePaddingArr.length;j++)
      {
          var num = parseInt(nums[j+1])
          sliceOriArr[j] = num
          if  (num == 0 ){
              if ((j+1) %2 == 0) {
                num = parseInt(height/2)

              }else{
                
                num = parseInt(width/2)
              }
          }
         slicePaddingArr[j] = num
     }
    }else{
      alert("图层名为："+layerName+"的九宫格格式不对！应为_9S:XX或:XX:XX:XX:XX");
      return;
    }
    
    var _obj = doc.activeLayer
    //左下左上，右上右下
    var selRegion = Array(
        Array(Array(0,slicePaddingArr[1]),Array(0, 0),Array(slicePaddingArr[0] , 0),Array(slicePaddingArr[0], slicePaddingArr[1])),
        Array(Array(width-slicePaddingArr[2],slicePaddingArr[1]),Array(width-slicePaddingArr[2], 0),Array(width , 0),Array(width, slicePaddingArr[1])),
        Array(Array(0,height),Array(0, height-slicePaddingArr[3]),Array(slicePaddingArr[0] , height-slicePaddingArr[3]),Array(slicePaddingArr[0], height)),
        Array(Array(width-slicePaddingArr[2],height),Array(width-slicePaddingArr[2], height-slicePaddingArr[3]),Array(width , height-slicePaddingArr[3]),Array(width, height)),
        );
    for (var i = 0;i<selRegion.length;i++)
    {
        doc.activeLayer = _obj;
        doc.selection.select(selRegion[i]);
        doc.selection.copy();
        var newStem = doc.paste();
        newStem.name = vaildName;
        var deltaX = 0;
        var deltaY = 0;
        if(selRegion[i][0][0] != 0){
            deltaX = - (width - slicePaddingArr[0]-slicePaddingArr[2]);
        }
        if(selRegion[i][1][1] != 0){
            deltaY = - (height - slicePaddingArr[1]-slicePaddingArr[3]);
        }
        newStem.translate(deltaX,deltaY);
    }
    _obj.visible = false;
    doc.mergeVisibleLayers();
    sceneData += "<arguments>";
    sceneData += "<string>" + sliceOriArr[0] + "</string>";
    sceneData += "<string>" + sliceOriArr[1] + "</string>";
    sceneData += "<string>" + sliceOriArr[2] + "</string>";
    sceneData += "<string>" + sliceOriArr[3] + "</string>";
    sceneData += "</arguments>";

    trim(doc);
    saveScenePng(doc, vaildName, true,true);
}