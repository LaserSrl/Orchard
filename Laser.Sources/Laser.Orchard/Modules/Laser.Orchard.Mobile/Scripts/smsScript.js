function ControllaMessaggio(event) {

    var maxLength = event.data.maxLength;
    var shortlinkExist = event.data.shortlinkExist;

    //console.log("maxLength=" + maxLength);
    //console.log("shortlinkExist=" + shortlinkExist);

    var $txtTesto = $(event.data.txtTesto);
    var $lblnumeroMessaggi = $(event.data.lblnumeroMessaggi);
    var $lblnumeroChar = $(event.data.lblnumeroChar);

    var ValidChars = '\\$@[]^_{|}~€àèìòùé+:£\r\n';
    var CharAcc = 'àèìòùé';
    var charLenght2 = '\\[]^{}|~€';

    //var JsPlaceHolder = event.data.JsPlaceHolder;

    var maxchar = 160
    var oldCount = 0;
    var IsNumber = 0;

    if (shortlinkExist == 'True') {
        maxchar = maxchar - 16;
    }

    //console.log("maxchar=" + maxchar);

    var Char;
    var conteggiocaratteri = 0;
    var MaxnumberOfSpedizioni = parseInt(maxLength / maxchar)

    //console.log("maxLength=" + maxLength);

    if (event.keyCode >= 37 && event.keyCode <= 40) { return; }

    for (var i = 0; i < $txtTesto.val().length ; i++) {
        conteggiocaratteri++;
        Char = $txtTesto.val().charAt(i);
        if (ValidChars.indexOf(Char) > -1) {
            //Il controllo deve essere effettuato solamente per i messaggi non concatenati
            if (maxchar >= maxLength) {

                if ((IsNumber >= (maxLength - 2) && IsNumber <= maxLength) && charLenght2.indexOf(Char) > -1) {
                    //console.log("if ((IsNumber >= 159 && IsNumber <= 160) && Char == ':')");
                    $txtTesto.val($txtTesto.val().substring(0, $txtTesto.val().length - 1));
                    break;
                }
            }

            if (charLenght2.indexOf(Char) > -1 || Char == '\\') {
                IsNumber = IsNumber + 2;
            } else {
                IsNumber = IsNumber + 1;
            }

        } else {
            IsNumber = IsNumber + 1;
        }
        //console.log("IsNumber=" + IsNumber);
        numberOfSpedizioni = CalcolaSpedizioni(IsNumber, maxLength, $lblnumeroChar, false, shortlinkExist);
        if (numberOfSpedizioni > MaxnumberOfSpedizioni) {
            //console.log("numberOfSpedizioni=" + numberOfSpedizioni);
            IsNumber = PulisciTesto(numberOfSpedizioni, IsNumber, maxLength, conteggiocaratteri, $txtTesto);
            break;
        }
    }

    //elimino dal conteggio i caratteri dei placeHolder
    /*if (JsPlaceHolder != "") {
        
        var iCharTotalPH = 0;
        var ArrPH = JsPlaceHolder.split(",");
        ArrPH = $.grep(ArrPH, function (n) { return (n) });
        for (i = 0; i < ArrPH.length; i++) {
            var phOcurrences = countOcurrences($txtTesto.val(), ArrPH[i])
            if (phOcurrences > 0) {
                var ichar1 = countOcurrences(ArrPH[i], '[')
                var ichar2 = countOcurrences(ArrPH[i], ']')
                var ichar3 = countOcurrences(ArrPH[i], '_')
                var icharspec = ichar1 + ichar2 + ichar3
                iCharTotalPH = iCharTotalPH + ((ArrPH[i].length - icharspec + (icharspec * 3)) * phOcurrences)
            }
        }
        IsNumber = IsNumber - iCharTotalPH
    }
    */

    //Calcola numero Spedizioni
    numberOfSpedizioni = CalcolaSpedizioni(IsNumber, maxLength, $lblnumeroChar, true, shortlinkExist)
    $lblnumeroMessaggi.text(numberOfSpedizioni);
    
    IsNumber = PulisciTesto(numberOfSpedizioni, IsNumber, maxLength, conteggiocaratteri, $txtTesto);

    $lblnumeroChar.val(IsNumber);
    
    if (IsNumber >= 0 && IsNumber <= 160)
        $lblnumeroMessaggi.removeClass("btn-danger");
    else
        $lblnumeroMessaggi.addClass("btn-danger");
}

function CalcolaSpedizioni(IsNumber, maxLength, $lblnumeroChar, bsetlblNumChar, shortlinkExist) {
    var MAX_CHAR = 160;
    var MAX_CHAR_CONCATENATO = 153;

    var CharNumber = IsNumber;

    if (shortlinkExist == 'True') {
        CharNumber = CharNumber + 16;
    }

    //console.log("IsNumber=" + IsNumber);
    //console.log("CharNumber=" + CharNumber);

    var numberOfSpedizioni = (CharNumber / MAX_CHAR);
    if (CharNumber > MAX_CHAR) {
        numberOfSpedizioni = (CharNumber / MAX_CHAR_CONCATENATO);
        if (CharNumber % MAX_CHAR_CONCATENATO > 0) {
            //se avessi modulo 0 significa che è lungo esattamente 2,3,4... SMS
            //se il modulo è maggiore di 0 significa che ho più testo es. ho il 3o SMS
            numberOfSpedizioni++;
        }
        if (bsetlblNumChar) {
            if (IsNumber < maxLength)
                $lblnumeroChar.text(IsNumber + 12);
            else
                $lblnumeroChar.text(IsNumber);
        }
    } else {
        if (IsNumber > 0) numberOfSpedizioni = 1;
        else numberOfSpedizioni = 0;
        if (bsetlblNumChar) $lblnumeroChar.text(IsNumber);
    }

    numberOfSpedizioni = parseInt(numberOfSpedizioni)

    return numberOfSpedizioni
}

function PulisciTesto(numberOfSpedizioni, IsNumber, maxLength, conteggiocaratteri, $txtTesto) {
    //console.log("call PulisciTesto");
    var CaratteriHeader = 0
    if (numberOfSpedizioni > 1)
        CaratteriHeader = ((numberOfSpedizioni) * 12)

    //console.log("conteggiocaratteri:" + conteggiocaratteri + " - IsNumber:" + IsNumber + " - maxLength:" + maxLength + " - CaratteriHeader:" + CaratteriHeader + " - (IsNumber + CaratteriHeader):" + (IsNumber + CaratteriHeader))

    if ((IsNumber + CaratteriHeader) >= maxLength || conteggiocaratteri >= maxLength) {
        if (IsNumber == maxLength) {
            $txtTesto.val($txtTesto.val().substring(0, conteggiocaratteri));
        } else {
            IsNumber = maxLength - CaratteriHeader;
            $txtTesto.val($txtTesto.val().substring(0, conteggiocaratteri - 1));
        }
    }
    else {

        $txtTesto.val($txtTesto.val().substring(0, conteggiocaratteri));
    }

    return IsNumber;
}

/***************************/

jQuery.fn.extend({
    insertAtCaret: function (myValue) {
        return this.each(function (i) {
            if (document.selection) {
                //For browsers like Internet Explorer
                this.focus();
                var sel = document.selection.createRange();
                sel.text = myValue;
                this.focus();
            }
            else if (this.selectionStart || this.selectionStart == '0') {
                //For browsers like Firefox and Webkit based
                var startPos = this.selectionStart;
                var endPos = this.selectionEnd;
                var scrollTop = this.scrollTop;
                this.value = this.value.substring(0, startPos) + myValue + this.value.substring(endPos, this.value.length);
                this.focus();
                this.selectionStart = startPos + myValue.length;
                this.selectionEnd = startPos + myValue.length;
                this.scrollTop = scrollTop;
            } else {
                this.value += myValue;
                this.focus();
            }
        });
    }
});
