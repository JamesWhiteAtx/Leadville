/*
* Selector seats
*
* Depends:
* jquery.js
* jquery.validate.js
*/

/*
* String extension func to convert to propercase 
*/
String.prototype.toProperCase = function () {
    return this.replace(/\w\S*/g, function (txt) { return txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase(); });
};

/*
* constructor for base object of selector
* prepares jQuery object for use by seletor
* @param container jQuery object
* @param caption string display description
*/
function SelObj(container, caption) {
    this.container = container;
    $(this.container).hide();
    this.caption = null;
    this.items = null;
    if (this.container) {
        this.container.addClass('sel-section');
        this.caption = $('<h2>').addClass('sel-caption').append(caption).appendTo(this.container);
        this.items = $('<div>').addClass('sel-items').data('name', name).appendTo(this.container);
    };
};

/*
* clears selector object
*/
SelObj.prototype.clear = function () {
    this.items.html('');
};
/*
* supresses selector object from display 
*/
SelObj.prototype.hide = function () {
    this.container.hide();
    this.notFinished();
};
/*
* displays sector object
*/
SelObj.prototype.show = function () {
    this.container.show();
};
/*
* indicates if selector object is visible
* returns boolean
*/
SelObj.prototype.isShowing = function () {
    return this.container.is(":visible");
};
/*
* updates selector object display to indicate loading
*/
SelObj.prototype.startLoad = function (elm) {
    (elm || this.items).addClass('loading');
};
/*
* updates selector object display to indicate not loading
*/
SelObj.prototype.endLoad = function () {
    this.container.removeClass('loading');
    this.container.find('.loading').removeClass('loading');
};
/*
* updates selector object display to highlighted state
*/
SelObj.prototype.addHighLight = function (elm) {
    elm.addClass('highlight');
};
/*
* updates selector object display to remove highlighted state
*/
SelObj.prototype.removeHighLight = function (elm) {
    var remove = (elm || this.container.find('.highlight'));
    remove.removeClass('highlight');
};
/*
* updates selector object display to finished state
*/
SelObj.prototype.finished = function () {
    this.container.find('h2').addClass('finished');
};
/*
* updates selector object display to remove finished state
*/
SelObj.prototype.notFinished = function () {
    this.container.find('h2').removeClass('finished');
};
/*
* executes ajax load of options
* @param url string url for ajax call
* @param parm nullable object data parameter for ajax call
* @param selElm jQuery object passed through
* @param loadFunc callback function passed through to load element
* @param pickedFunc callback function passed through event option selection
*/
SelObj.prototype.loadList = function (url, parm, selElm, loadFunc, pickedFunc) {
    var allParms = true;
    for (var key in parm) {
        if (!parm[key]) {
            allParms = false;
            break;
        };
    };
    var self = this;
    if (allParms) {
        this.startLoad(selElm);
        $.ajax({
            dataType: "json"
                    , url: url
                    , data: parm
                    , success: $.proxy(function (result) {
                        loadFunc.call(this, selElm, result, pickedFunc);
                    }, this)
                    , error: function (jqXHR, textStatus, errorThrown) { alert(textStatus + ' ' + errorThrown); }
                    , complete: $.proxy(function () { this.endLoad(); }, this)
        });
    } else {
        loadFunc.call(this, selElm, [], pickedFunc);
    };
};

/* SelMenuObj
* Selector object to display dropdown menu options
*/
SelMenuObj.prototype = new SelObj();
SelMenuObj.prototype.constructor = SelMenuObj;
function SelMenuObj(container, caption) {
    SelObj.call(this, container, caption);
};

/*
* adds an option to a selectable dropdown menu interface
* @param elem jQuery object drop down menu element
* @param id string new element value
* @param text string new element display text
*/
SelMenuObj.prototype.addSelOpt = function (elem, id, text) {
    elem.append($('<option>', { value: id, text: text }));
};
/*
* add a selectable dropdown menu interface to the selector object
* @param name string 
* @returnd jQuery object new selectable dropdown menu 
*/
SelMenuObj.prototype.addSel = function (name) {
    var sel = $('<select>').data('name', name).attr('disabled', 'disabled').appendTo(this.items);
    this.addSelOpt(sel, '', '-- Select ' + name + ' --');
    $('<span>').addClass('status').appendTo(this.items);
    return sel;
};
/*
* loads options in a selectable dropdown menu 
* @param elem jQuery object drop down menu element
* @param items iteratable object of elelment type {id, name}
* @param changeFunc callback function invoked when option selected
*/
SelMenuObj.prototype.loadSel = function (selElm, items, changeFunc) {
    selElm.unbind('change.selector');
    selElm.empty();
    this.addSelOpt(selElm, '', '-- Select ' + selElm.data('name') + ' --');
    if (items.length > 0) {
        $(items).each($.proxy(function (i, o) {
            this.addSelOpt(selElm, o.id, o.name);
        }, this));

        var opts = selElm.find('option').length;
        if (opts > 1) {
            if (opts == 2) {
                selElm.find('option:eq(1)').attr('selected', 'selected');
            };
            if (changeFunc) {
                selElm.bind('change.selector', function () {
                    changeFunc($(this).find("option:selected").val());
                });
            };
            selElm.removeAttr('disabled');
            selElm.change();
            selElm.focus();
            this.removeHighLight();
            this.addHighLight(selElm);
        }
    } else {
        selElm.attr('disabled', 'disabled');
        this.removeHighLight(selElm);
        if (changeFunc) {
            changeFunc('');
        };
    };
};
SelMenuObj.prototype.loadSelList = function (url, parm, selElm, pickedFunc) {
    this.loadList(url, parm, selElm, this.loadSel, pickedFunc);
};

/* SelBoxObj
* Selector object to display list of images 
*/
SelBoxObj.prototype = new SelObj();
SelBoxObj.prototype.constructor = SelBoxObj;
function SelBoxObj(container, caption) {
    SelObj.call(this, container, caption);
};
SelBoxObj.prototype.pickBox = function (elem, pickFunc) {
    var leaCD = null;

    if (elem) {
        this.finished();
        this.endLoad();
        leaCD = $(elem).data('prodcd');
        this.removeHighLight();
        this.addHighLight($(elem).find('div.leather-box'));
        elem.focus();
    } else {
        leaCD = null;
        this.notFinished();
        this.removeHighLight();
    };
    pickFunc(leaCD);
};
SelBoxObj.prototype.loadBox = function (elm, items, pickFunc) {
    elm.html('');
    var links = [];
    var self = this;
    $(items).each(function () {
        var link = $('<a>', { 'href': '#', 'title': this.prodCD, 'data-prodcd': this.prodCD })
                    .click(function () { self.pickBox(this, pickFunc); })
                    .appendTo(elm);
        links.push(link);

        var img = $('<img>', { 'id': this.prodCD, 'src': this.src, 'alt': this.colorCD });
        if (self.altSrc) {
            img.error(function () {
                $(this).unbind('error');
                this.src = self.altSrc;
            });
        };

        $('<div>').addClass('leather-box')
                    .append($('<div>').addClass('leather-descr').append(this.color + ' ' + this.prodCD))
                    .append($('<div>').append(img))
                    .append($('<div>').append(this.advice))
                    .appendTo(link);
    });
    if (links.length == 1) {
        this.pickBox(links[0], pickFunc);
    } else {
        this.pickBox(null, pickFunc);
    };
};
SelBoxObj.prototype.loadBoxList = function (url, parm, altSrc, pickedFunc) {
    this.altSrc = altSrc;
    this.loadList(url, parm, this.items, this.loadBox, pickedFunc);
};

/* SelRadioObj
*
*
*/
SelRadioObj.prototype = new SelObj();
SelRadioObj.prototype.constructor = SelFormObj;
function SelRadioObj(container, caption) {
    SelObj.call(this, container, caption);
    this.lastParm = {};
};
SelRadioObj.prototype.loadRdos = function (elm, items, pickFunc) {
    elm.html('');
    var first;
    var self = this;
    $(items).each(function () {
        var num = Number(this.id);
        if (!isNaN(num)) {
            var rdo = $('<input>', { type: "radio", id: "rows-" + this.id, name: "sel-rows", value: this.id })
                .change(function () { pickFunc($(this).val()); });
            if (!first) {
                first = rdo;
                rdo.prop('checked', true)
            };
            rdo.appendTo(elm);
            $('<label>').attr('for', "rows-" + this.id).append(this.name).appendTo(elm);
        };
    });
    if (first) {
        pickFunc( first.val() );
    } else {
        pickFunc(null);
    };
};
SelRadioObj.prototype.loadRdoList = function (url, parm, pickedFunc) {
    if (this.lastParm.patternCD !== parm.patternCD) {
        this.lastParm.patternCD = parm.patternCD;
        this.loadList(url, parm, this.items, this.loadRdos, pickedFunc);
    } else {
        pickedFunc(this.items.find("input[type='radio'][name='sel-rows']:checked").val());
    }
};

/* SelFormObj
* Selector object to display form and inputs
*/
SelFormObj.prototype = new SelObj();
SelFormObj.prototype.constructor = SelFormObj;
function SelFormObj(container, caption, submitUrl) {
    SelObj.call(this, container, caption);

    var form = $('<form>', { id: 'bigco-form', method: 'POST', action: submitUrl }).appendTo(this.items);

    this.addInput = function (caption, name, type) {
        $('<div>').addClass('sel-input')
                .append($('<label>', { 'for': name }).append(caption))
                .append($('<input/>', { id: name, name: name, type: type }))
                .appendTo(form);
    }
    this.addInput("Member Email", "email", 'email');
    this.addInput("First Name", "firstName", 'text');
    this.addInput("Last Name", "lastName", 'text');
    this.addInput("Phone", "phone", 'tel');

    this.addHidden = function (name) {
        return $('<input/>', { id: name, name: name, type: 'hidden' }).appendTo(form);
    }
    this.carInpt = this.addHidden("carcd");
    this.intInpt = this.addHidden("intcd");
    this.prodInpt = this.addHidden("prodcd");
    this.rowsInpt = this.addHidden("rows");

    $('<div>')
        .append($('<input/>', { id: 'bigco-submit', name: 'bigco-submit', value: 'Order Now', type: 'submit' }))
        .appendTo(form);

//    $.validator.addMethod("bigco", function (value, element) {
//        var regex = /^([a-zA-Z0-9_\.\-\+])+\@bigco\.com$/i;
//        return (this.optional(element) || (regex.test(value)));
//    }, "Please enter only a bigco.com email address");

    $(form).validate({
        rules: {
            email: { required: true,
                email: true
                //bigco: true
            },
            firstName: { required: true },
            lastName: { required: true },
            phone: { required: true }
        }
    });
};

/* CCSelector
* constructor for selector control object. Coordinates flow of 
* @param opts object setup options:
*   carCont: jQuery object used to display car information
*   intColCont: jQuery object used to display interior color information
*   leatherCont: jQuery object used to display leater color information
*   memberCont: jQuery object used to display member input information
*   fillerElm: jQuery object used to display optional filler information
*   makesUrl: string url to return json list of Makes
*   yearsUrl: string url to return json list of Years
*   modelsUrl: string url to return json list of Models
*   bodiesUrl: string url to return json list of Boy styles
*   patternsUrl: string url to return json list of available patterns
*   intColsUrl: string url to return json list of interior color options
*   leathersUrl: string url to return json list of available leather seats
*   submitUrl: string url for redirection when user input is complete
*   changeFunc: callback function invoked on selection of each level
* @returns CCSelector object
*/
function CCSelector(opts) {
    var makesUrl = opts.makesUrl;
    var yearsUrl = opts.yearsUrl;
    var modelsUrl = opts.modelsUrl;
    var bodiesUrl = opts.bodiesUrl;
    var patternsUrl = opts.patternsUrl;
    var intColsUrl = opts.intColsUrl;
    var leathersUrl = opts.leathersUrl;
    var altImgSrc = opts.altImgSrc;
    var rowsUrl = opts.rowsUrl;
    var fillerElm = opts.fillerElm;
    var submitUrl = opts.submitUrl;
    var changeFunc = opts.changeFunc;

    var carObj = new SelMenuObj(opts.carCont, "Find Your Vehicle");
    var intColObj = new SelMenuObj(opts.intColCont, "Match Factory Interior Color");
    var leatherObj = new SelBoxObj(opts.leatherCont, "Select Your Seat Color");
    var rowsObj = new SelRadioObj(opts.rowsCont, "Select Number of Rows");
    var memberObj = new SelFormObj(opts.memberCont, "Membership Details", submitUrl);

    carObj.clear();
    carObj.selMake = carObj.addSel('Make');
    carObj.selYear = carObj.addSel('Year');
    carObj.selModel = carObj.addSel('Model');
    carObj.selBody = carObj.addSel('Body');
    carObj.selTrim = carObj.addSel('Trim Level');

    carObj.makeCD = null;
    carObj.yearCD = null;
    carObj.modelCD = null;
    carObj.carCD = null;
    carObj.patternCD = null;

    intColObj.hide();
    intColObj.clear();
    intColObj.selIntCol = intColObj.addSel('Interior Color');

    leatherObj.hide();
    leatherObj.clear();

    rowsObj.hide();
    rowsObj.clear();

    this.getCarObj = function () { return carObj; };
    this.getIntColObj = function () { return intColObj; };
    this.getLeatherObj = function () { return leatherObj; };
    this.getRowsObj = function () { return rowsObj; };
    this.getMemberObj = function () { return memberObj; };

    var self = this;

    loadMakes();

    function loadMakes() {
        carObj.show();
        carObj.makeCD = null;
        carObj.loadSelList(makesUrl, null, carObj.selMake, makePicked);
        callChange();
    };

    function makePicked(makeCD) {
        carObj.makeCD = makeCD;
        loadYears(carObj.makeCD);
    };

    function loadYears() {
        carObj.yearCD = null;
        carObj.loadSelList(yearsUrl, { makeCD: carObj.makeCD }, carObj.selYear, yearPicked);
    };

    function yearPicked(yearCD) {
        carObj.yearCD = yearCD;
        loadModels();
    };

    function loadModels() {
        carObj.modelCD = null;
        carObj.loadSelList(modelsUrl, { makeCD: carObj.makeCD, yearCD: carObj.yearCD }, carObj.selModel, modelPicked);
    };

    function modelPicked(modelCD) {
        carObj.modelCD = modelCD;
        loadBodys();
    };

    function loadBodys() {
        carObj.carCD = null;
        carObj.loadSelList(bodiesUrl, { makeCD: carObj.makeCD, yearCD: carObj.yearCD, modelCD: carObj.modelCD },
                    carObj.selBody, bodyPicked);
    };

    function bodyPicked(carCD) {
        carObj.carCD = carCD;
        loadPatterns();
    };

    function loadPatterns() {
        carObj.patternCD = null;
        carObj.loadSelList(patternsUrl, { carCD: carObj.carCD }, carObj.selTrim, patternPicked);
    };

    function patternPicked(patternCD) {
        carObj.patternCD = patternCD;
        loadIntCols();
    };

    function loadIntCols() {
        intColObj.intColCD = null;
        intColObj.loadSelList(intColsUrl, { carCD: carObj.carCD, patternCD: carObj.patternCD }, intColObj.selIntCol,
                    function (intColCD) {
                        intColPicked(intColCD);
                        carObj.removeHighLight();
                    }
                );
        if (carObj.patternCD) {
            carObj.finished();
            intColObj.show();
        } else {
            carObj.notFinished();
            intColObj.hide();
        };
        callChange();
    };

    function intColPicked(intColCD) {
        intColObj.intColCD = intColCD;
        loadLeather();
    };

    function loadLeather() {
        leatherObj.leatherCD = null;

        leatherObj.loadBoxList(leathersUrl, { carCD: carObj.carCD, patternCD: carObj.patternCD, intColCD: intColObj.intColCD }, altImgSrc,
                    function (elm) {
                        leatherPicked(elm);
                        intColObj.removeHighLight();
                    }
                );

        if (intColObj.intColCD) {
            intColObj.finished();
            fillerElm.hide();
            leatherObj.show();
        } else {
            intColObj.notFinished();
            leatherObj.hide();
            fillerElm.show();
        };
        callChange();
    };

    function leatherPicked(leatherCD) {
        leatherObj.leatherCD = leatherCD;
        loadRows();
    };

    function loadRows() {
        rowsObj.rowsCD = null;
        rowsObj.loadRdoList(rowsUrl, { carCD: carObj.carCD, patternCD: carObj.patternCD }, rowsPicked);
        if (leatherObj.leatherCD) {
            leatherObj.finished();
            rowsObj.show();
        } else {
            leatherObj.notFinished();
            rowsObj.hide();
        };
        callChange();
    };

    function rowsPicked(rowsCD) {
        rowsObj.rowsCD = rowsCD;

        if ( (carObj.carCD) && (intColObj.intColCD) && (leatherObj.leatherCD) && (rowsObj.rowsCD) ) {
            memberObj.carInpt.val(carObj.carCD);
            memberObj.intInpt.val(intColObj.intColCD);
            memberObj.prodInpt.val(leatherObj.leatherCD);
            memberObj.rowsInpt.val(rowsObj.rowsCD);
            memberObj.show();
        } else {
            memberObj.hide();
        };

        callChange();
    };

    function callChange() {
        if (typeof changeFunc == 'function') {
            changeFunc(self);
        };
    };

}; // function CCSelector()
