function keyPressUpper(event, selectr) {
    if(null !== String.fromCharCode(event.which).match(/[a-z]/g)) {
        event.preventDefault();
        $(selectr).val($(selectr).val() + String.fromCharCode(event.which).toUpperCase());
    }
};

function upperTextInputs() {
    $('input[type="text"].text-input-upper, .text-input-upper input[type="text"]').keypress(function(event) {
        keyPressUpper(event, this);
    });
    //$('input[type="text"].text-input-upper, .text-input-upper input[type="text"]').keyup(function () {this.value = this.value.toUpperCase(); });
};

function onEditBindUpper(e) {
    upperTextInputs();
};

function bindTGridUpper() {
    $('.t-grid').bind('edit', onEditBindUpper);
};

function readOnlyTextInputs() {
    $('.readonly-field input[type="text"]').attr("disabled", "disabled");
};

function tGridError(e) {
    if (e.textStatus == "modelstateerror" && e.modelState) {
        // accumulate the error messages in the "message" variable
        var message = "Errors:\n";

        // iterate over each key value pair
        $.each(e.modelState, function (key, value) {
            if (value.errors) {
                // iterate over all errors and accumulate them in the "message" variable
                $.each(value.errors, function (index, error) {
                    message += error + "\n";
                });
            }
        });
        e.preventDefault();
        alert(message);
    }
};

function tGridBoundRetPage(e) {
    var gridElem = $(e.target);
    tGridLinksRetPage(gridElem);
};

function tGridLinksRetPage(gridElem, linkSel) {
    var urlReturn = tGridRetUrl(gridElem.data('tGrid'));
    if ($.type(urlReturn) == "string") {
        linkSel = (linkSel) ? linkSel : "a.t-grid-retpage";
        $(gridElem).find(linkSel).each(function () {
            var link = $(this);
            var href = link.attr("href");
            href = hrefParms(href, { urlReturn: urlReturn });
            link.attr("href", href);
        });
    };
};

function tGridSelRetUrl(gridSel) {
    return tGridRetUrl($(gridSel));
};

function tGridRetUrl(tGrid) {
    var urlReturn;
    if ($.type(tGrid) == "object") {
        var currentGridPage = tGrid.currentPage;
        urlReturn = window.location.pathname;
        var parms = parmObj(window.location.search);
        if (currentGridPage > 1) {
            parms.page = currentGridPage;
        } else {
            delete parms.page;
        };
        urlReturn = hrefParms(urlReturn, parms);
    };
    return urlReturn;
};

function tGridNameDelRebind(e) {
    if (e.name == "delete") {
        var grid = $(e.target).data('tGrid');
        grid.ajaxRequest();
    };
};

function tGridReRefresh(e, text, title) {
    var gridElm = $(e.target);
    var container = gridElm.find("div.t-grid-pager.t-grid-top div.t-status");
    var oldLink = container.find("a.t-icon.t-refresh");
    var href = oldLink.attr("href");
    var newLink = $("<a href='/' class='t-refresh strengthened'>Refresh</a>");
    newLink.attr("href", href);
    if (text) {
        oldLink.text(text);
        newLink.text(text);
    };
    if (title) {
        oldLink.attr("title", title);
        newLink.attr("title", title);
    };
    container.append(newLink);
};

function tGridCmdTxt(gridElem, cmdClass, text) {
    $(gridElem).find("a."+cmdClass).each(function () {
        var anch = $(this);
        anch.html(anch.html().replace(anch.text(), text))
    });
};

function tGridInsertCmdTxt(gridElem, text) {
    tGridCmdTxt(gridElem, "t-grid-add", text);
};

function tGridAddExpExcel(e, href) {
    var gridElm = $(e.target);
    if (gridElm.length < 1) { return null; }
    
    var toolbar = gridElm.find('div.t-toolbar.t-grid-toolbar.t-grid-top');
    if (toolbar.length < 1) {
        toolbar = $("<div class='t-toolbar t-grid-toolbar t-grid-top'></div>");
        gridElm.prepend(toolbar);
    };

    var excLink = toolbar.find('a.tgrid-excel-export');

    if (excLink.length < 1) {
        excLink = $('<a/>');
        excLink.addClass("t-button-icontext t-button tgrid-excel-export")
            .append($('<span>').addClass('t-icon').addClass('t-arrow-down'))
            .append("Excel Export");
        toolbar.append(excLink);
    };

    var gridObj = gridElm.data('tGrid');
    var colStr = '';
    for (var i = 0, len = gridObj.columns.length; i < len; i++) {
        var memb = gridObj.columns[i].member;
        if ($.type(memb) !== "undefined") {
            colStr += memb + ':' + gridObj.columns[i].title + ';';
        };
    };
    var exparms = {
        columns: colStr,
        orderBy: (gridObj.orderBy || '~'),
        filter: (gridObj.filterBy || '~')
    };
    var link = document.createElement('a');
    link.href = href;
    var path = link.pathname;
    var parms = parmObj(link.search);

    $.extend(parms, exparms);
    excLink.attr("href", path + '?' + $.param(parms))

};

function wireGridUpDn(e, upDnSel, upUrl, dnUrl) {
    var grid = $(e.target).data('tGrid');
    $(upDnSel).click(function (e) {
        e.preventDefault();

        var upDnElm = $(this);
        var url;
        if (upDnElm.hasClass("updn-up")) {
            url = upUrl;
        } else if (upDnElm.hasClass("updn-dn")) {
            url = dnUrl;
        };

        var jobID = upDnElm.data('id');
        var jobSeq = upDnElm.data('seq');
        $.getJSON(url, { id: jobID, seq: jobSeq }, function (data) {
            if (data) {
                if (data.result) {
                    grid.ajaxRequest();
                } else if (data.message) {
                    alert(data.message);
                };
            };
        });

    });
};

function inputAndSpanValue(id, spanId, value, defaultValue) {
    var input = $("#" + id);
    var span = $("#" + spanId);
    var theValue;
    if ((!(value) || (value === "")) && (defaultValue)) {
        theValue = defaultValue;
    } else {
        theValue = value;
    }
    if (input) { input.val(theValue); };
    if (span) { span.text(theValue); }
};

function inputValueAndSpanValue(id, spanId, inputValue, spanValue) {
    var input = $("#" + id);
    var span = $("#" + spanId);
    if (input) { input.val(inputValue); };
    if (span) { span.text(spanValue); }
}

function isEmpty(x, p) { for (p in x) return !1; return !0 };

function pruneObj(obj) {
    var props = $.map(obj, function (value, key) { return key; });
    if (props.length > 0) {
        for (var i = 0; i < props.length; i++) {
            if (!obj[props[i]]) {
                delete obj[props[i]];
            };
        };
    };
    return obj;
};

function parmObj(parmStr) {
    var urlParams = {};
    var match,
    pl = /\+/g,  // Regex for replacing addition symbol with a space
    search = /([^&=]+)=?([^&]*)/g,
    decode = function (s) { return decodeURIComponent(s.replace(pl, " ")); },
    query = parmStr.substring(1);
    while (match = search.exec(query))
        urlParams[decode(match[1])] = decode(match[2]);
    return urlParams;
};

function urlParm() { return parmObj(window.location.search); };

function hrefParms(href, exparms) {
    var url = document.createElement('a');
    url.href = href;
    var parms = parmObj(url.search);
    $.extend(parms, exparms);
    if (isEmpty(parms)) {
        return href;
    } else {
        return url.pathname + '?' + $.param(parms);
    }
};

function appendProdImgLink(elem, imgHref, descr, srcPath) {
    elem.append(
        $('<a/>', {
            href: imgHref,
            target: "_blank",
            title: descr,
            "class": "image-link-apnd"
        }).append(
            $('<img/>', {
                src: srcPath,
                alt: descr
            })
        )
    );
};

function clearErrorValidation(idFor) {
    var sel = '[data-valmsg-for="' + idFor + '"]';
    $(sel)
    .removeClass("field-validation-error")
    .addClass("field-validation-valid")
    .empty();
};

function setErrorValidation(idFor, errMessage) {
    var sel = '[data-valmsg-for="' + idFor + '"]';
    $(sel)
    .removeClass("field-validation-valid")
    .addClass("field-validation-error")
    .text(errMessage);
};

function addLoadingClass(elem) {
    if (elem) {elem.addClass("ajax-loading");}
};

function removeLoadingClass(elem) {
    if (elem) {elem.removeClass("ajax-loading");}
};

function evalString(eval, defVal) {
    var retVal = evalValType(eval, "string", defVal);
    if (!retVal) {retVal = undefined;}
    return retVal;
};

function evalNumber(eval, defVal) {
    var retVal = evalValType(eval, "number", defVal);
    if (!retVal) { retVal = undefined; }
    return retVal;
};

function evalBoolean(eval, defVal) {
    if ($.type(defVal) !== "boolean") {defVal = false;};
    return evalValType(eval, "boolean", defVal);
};

function evalObject(eval, defVal) {
    return evalValType(eval, "object", defVal);
};

function evalValType(eval, type, defVal) {
    var retVal;
    if ($.type(eval) === type) { retVal = eval; }
    else if (typeof eval == "function") {
        var funcRes = eval();
        if ($.type(funcRes) === type) { retVal = funcRes; };
    };
    if (($.type(retVal) !== type) && ($.type(defVal) !== "undefined")) {
        retVal = evalValType(defVal, type);
    }
    return retVal;
};

function linkUrl(url, parm1, parm2, parm3) {
    if (parm1) {
        url = url.replace("_parm_", parm1);
    };
    if (parm2) {
        url = url.replace("_parm_", parm2);
    };
    if (parm3) {
        url = url.replace("_parm_", parm3);
    };
    $(location).attr('href', url);
};

// lookup

function loadCustLocs(custId, passedLocId, url, locSel, locDisp) {
    var locDDL = $(locSel);
    if (locDDL) {
        locDDL.val("");
        locDDL.empty();
        var emptyText;
        if (custId) {
            emptyText = "-- Select " + locDisp + " --";
        } else {
            emptyText = "--"
        };
        locDDL.append($('<option/>', { value: "" }).html(emptyText));
        if (custId) {
            $.getJSON(url, { custID: custId }, function (result) {
                var items = 0;
                var selId = null;
                $(result).each(function () {
                    locDDL.append($('<option/>', { value: this.LocID }).html(this.Display));
                    items++;
                    if (selId == null) {
                        selId = this.LocID;
                    };
                    if (this.LocID === passedLocId) {
                        selId = this.LocID;
                    };
                });
                if (selId === passedLocId) {
                    locDDL.val(selId);
                } else if ((items == 1) && (selId)) {
                    locDDL.val(selId);
                }
            });
        };
    };
};


// Unobtrusive

$(document).on("click", ".cst-clearbtn", function (e) {
    var selector = $(this).data("clearbtn-sel");
    var elem = $(selector)
    if (elem.length > 0) {
        elem.val("");
        elem.focus();
    };
});

$(document).on("click", ".cst-linkval", function (e) {
    e.preventDefault();
    var link = document.createElement('a');
    link.href = $(this).data("linkval-url");
    var parms = parmObj(link.search);
    var path = link.pathname;
    var url = path;
    var dataSel;
    var selector;
    var val;
    var names = $.map(parms, function (value, key) { return key; });
    if (names.length > 0) {
        var values = $.map(parms, function (value, key) { return value; });
        for (var i = 0; i < values.length; i++) {
            if (values[i].indexOf("linkval-token-") >= 0) {
                selector = $(this).data(values[i]);
                val = $(selector).val();
                if (val) {
                    parms[names[i]] = val;
                } else {
                    delete parms[names[i]];
                };
            };
        };
        var search = $.param(parms);
        if (search) {
            url = url + '?' + search;
        };
    };

    selector = $(this).data("linkval-token-id");
    if (selector) {
        val = $(selector).val();
        url = url.replace("linkval-token-id", val);
    };

    $(location).attr('href', url);
    return false;
});

// bubbles

function viewSettingsBubble(bubbleElm) {
    bubbleElm.CreateBubblePopup({
        openingDelay: 800,
        width: 400,
        selectable: true,
        alwaysVisible: true,
        position: 'left', align: 'top',
        innerHtml: '<img src="/Content/Images/loading.gif" style="border:0px; vertical-align:middle; margin-right:10px; display:inline;" />loading!',
        themeName: 'black',
        themePath: '/Content/jquerybubblepopup-themes',
        afterShown: function () {
            if (bubbleElm.IsBubblePopupOpen()) {
                $.getJSON('/Lookup/_SettingsHtml', function (data) {
                    bubbleElm.SetBubblePopupInnerHtml(data, false); //false -> it shows new innerHtml but doesn't save it, then the script is forced to load everytime the innerHtml... 
                });
            };
        }
    });
};

$(document).on("mouseenter", "a.image-link", function (e) {
    var imgLink = $(this);
    var imgId = imgLink.data('img-id');
    if (imgId) {
        var bub = imgLink.IsBubblePopupOpen();
        if (bub == null) {
            var imgHref = imgLink.attr('href');
            imgHref = ((imgHref) ? imgHref : '/Product/ViewImage/' + imgId);
            var imgTtl = imgLink.data('img-title');
            imgTtl = ((imgTtl) ? imgTtl : '');
            imgLink.CreateBubblePopup({
                openingDelay: 400,
                //width: 400,
                selectable: true,
                alwaysVisible: true,
                position: 'right', align: 'middle',
                innerHtml: '<button data-img-id = "' + imgId +
                    '" data-img-href = "' + imgHref + 
                    '" data-img-title = "' + imgTtl + 
                    '", class="t-button-icon t-button t-button-bare image-link-button" title="Preview Image"><span class="t-search t-icon"></span></button>',
                themeName: 'black',
                themePath: '/Content/jquerybubblepopup-themes'
            });
            imgLink.ShowBubblePopup();
        };
    };
});

$(document).on("click", ".image-link-button", function (e) {
    var imgBtn = $(this);
    var imgId = imgBtn.data('img-id');
    var imgHref = imgBtn.data('img-href');
    if ((imgId) && (imgHref)) {
        var imgLinkId = "img_link_div_" + imgId;
        var imgLinkDiv = $("#" + imgLinkId)
        if (imgLinkDiv.length < 1) {
            var imgLinkDiv = $('<div />').appendTo('body');
            imgLinkDiv.attr('id', imgLinkId);
            imgLinkDiv.html('<iframe src="' + imgHref + '" style="width:100%; height:100%;" frameborder="1"></iframe>');
        };
        var exst = $(imgLinkDiv).is(':data(dialog)');
        if (exst == false) {
            var imgTtl = imgBtn.data('img-title');
            imgTtl = ((imgTtl) ? imgTtl : 'View Image');
            $(imgLinkDiv).dialog({
                resizable: true,
                height: 300,
                width: 500,
                title: imgTtl,
                position: { my: "left+3 bottom-3", of: e, collision: "fit" }
            });
        };
        var isOpn = $(imgLinkDiv).dialog("isOpen");
        if (isOpn == false) {
            $(imgLinkDiv).dialog("open");
        };
    } else {
        altert("Sorry - no image link ID");
    }
});

