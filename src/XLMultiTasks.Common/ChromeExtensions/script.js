var count = 0;
var jq = null;
var downloadAllVideosTimeout = 5000 * 6;
var pauseVideoTimeout = 5000;
var ajaxComplete = true;
var mockWaitSecond = 10;
Number.prototype.myPadding = function () {
    var number = this.valueOf();
    var length = 2;
    var str = '' + number;
    while (str.length < length) {
        str = '0' + str;
    }
    return str;
};

Date.prototype.format = function (fmt) { //author: meizz 
    var o = {
        "M+": this.getMonth() + 1, //月份 
        "d+": this.getDate(), //日 
        "H+": this.getHours(), //小时 
        "m+": this.getMinutes(), //分 
        "s+": this.getSeconds(), //秒 
        "q+": Math.floor((this.getMonth() + 3) / 3), //季度 
        "S": this.getMilliseconds() //毫秒 
    };
    if (/(y+)/.test(fmt))
        fmt = fmt.replace(RegExp.$1, (this.getFullYear() + "").substr(4 - RegExp.$1.length));
    for (var k in o)
        if (new RegExp("(" + k + ")").test(fmt))
            fmt = fmt.replace(RegExp.$1, (RegExp.$1.length == 1) ? (o[k]) : (("00" + o[k]).substr(("" + o[k]).length)));
    return fmt;
};


function log(message) {
    console.log("[x]" + message);
}

function randomIntFromInterval(min, max)
{
    return Math.floor(Math.random() * (max - min + 1) + min);
}

var serviceUrl = "http://localhost:12345/Api/PS/AddTask";
function callTaskApi(taskDto, callback) {
    log('<< callTaskApi start!');
    ajaxComplete = false;
    jq.ajax({
        type: "POST",
        url: serviceUrl,
        data: taskDto,
        dataType: 'json',
        success: function (mr) {
            ajaxComplete = true;
            try {
                log(mr.Message + ' => download use seconds: ' + mr.Data);
                mockWaitSecond = mr.Data;
            } catch (e) {
                mockWaitSecond = randomIntFromInterval(1, 20);
                log(e);
            }
            callback();
        },
        error: function () {
            ajaxComplete = true;
            log("error");
            callback();
        }
    });
}

function getVideoSrc() {
    var link = $('#vjs_video_3_html5_api');
    if (!link.length) {
        //try fix get src error(domId changed) 
        link = $('video');
    }
    return link.attr('src');
}

function pauseVideo() {

    var $pauseButton = $('#play-control');
    if ($pauseButton) {
        $pauseButton.click();
    } else if ($pauseButton.length === 1) {
        $pauseButton.click();
    }
}

function getCourseName() {
    var courseName = $('#course-title-link').text();
    courseName = courseName.replace(/[\/:?><]/g, '');
    return courseName;
}

function getSectionDom() {
    var folderDom = $('li.selected')
      .parents('ul')
      .prev('header')
      .children('div')
      .eq(1);
    return folderDom;
}

function getSaveFilePath() {
    var link = getVideoSrc();
    var courseName = getCourseName();
    // log(courseName);

    var folderDom = getSectionDom();
    var sectionName = folderDom.find('h2').text();
    var sectionIndex = (folderDom.parents('section.module.open').eq(0).index() + 1).myPadding();
    var saveFolder = sectionIndex + ' - ' + sectionName;
    saveFolder = saveFolder.replace(/[\/:?><]/g, '');
    // log(saveFolder);

    var rawFileName = $('#module-clip-title').text().split(' : ').pop().trim();
    var fileIndex = ($('li.selected').eq(1).index() + 1).myPadding();
    var saveFileName = fileIndex + ' - ' + rawFileName + '.' + link.split('?')[0].split('.').pop();
    saveFileName = saveFileName.replace(/[\/:?><]/g, '');
    // log(saveFileName);

    // log('processing => ' + courseName + ' ' + sectionIndex + ' - ' + fileIndex);
    return 'Pluralsight/' + courseName + '/' + saveFolder + '/' + saveFileName;
}

function downloadCurrentVideo() {
    var link = getVideoSrc();
    // log('downloadCurrentVideo: ');
    log(link);
    return;
}

function clickNext() {
    log('click next link at: ' + new Date().format("HH:mm:ss"));
    $('#next-control').click();
    setTimeout(pauseVideo, pauseVideoTimeout);
    setTimeout(downloadAllVideos, pauseVideoTimeout + 1000);
}

function downloadAllVideos() {

    if (!ajaxComplete) {
        log("not complete, wait...");
        setTimeout(downloadAllVideos, 5000);
        return;
    }

    var link = getVideoSrc();
    var saveFilePath = getSaveFilePath();

    var folderDom = getSectionDom();
    var sectionName = folderDom.find('h2').text();
    var finalFolderName = $('section:last').find('h2').text();
    var rawFileName = $('#module-clip-title').text().split(' : ').pop().trim();
    var finalFileName = $('section:last').find('li:last').find('h3').text();

    log('----');
    log(saveFilePath);
    //log(link);

    callTaskApi({ SaveFilePath: saveFilePath, Link: link }, function () {

        if (sectionName === finalFolderName && rawFileName === finalFileName) {
            alert("Full Course Downloaded!");
        } else {
            //log(rawFileName + ' complete, should click next in seconds: ' + ((downloadAllVideosTimeout) / 1000 + mockWaitSecond));
            //setTimeout(clickNext, downloadAllVideosTimeout + mockWaitSecond * 1000);
            log('callApiTask [' + rawFileName + '] complete use '+ mockWaitSecond + ' seconds, click next in  ' + ((downloadAllVideosTimeout) / 1000));
            setTimeout(clickNext, downloadAllVideosTimeout);
        }
    });
}



$(function () {
    $(document).keypress(function (e) {

        jq = $;

        if (e.which === 115 || e.which === 83) {
            // keypress `s`
            // log('s => current');
            downloadCurrentVideo();
        } else if (e.which === 97 || e.which === 65) {
            // keypress `a`
            count = 0;
            // log('a => all');
            downloadAllVideos();
        }
    });
});
