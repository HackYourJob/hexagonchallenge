namespace Fable.Import
open System
open System.Text.RegularExpressions
open Fable.Core
open Fable.Import.JS
open Fable.Import.Browser

module AceAjax =
    type [<AllowNullLiteral>] Delta =
        abstract action: string with get, set
        abstract range: Range with get, set
        abstract text: string with get, set
        abstract lines: ResizeArray<string> with get, set

    and [<AllowNullLiteral>] EditorCommand =
        abstract name: string with get, set
        abstract bindKey: obj with get, set
        abstract exec: Function with get, set
        abstract readOnly: bool option with get, set

    and [<AllowNullLiteral>] CommandManager =
        abstract byName: obj with get, set
        abstract commands: obj with get, set
        abstract platform: string with get, set
        abstract addCommands: commands: ResizeArray<EditorCommand> -> unit
        abstract addCommand: command: EditorCommand -> unit
        abstract exec: name: string * editor: Editor * args: obj -> unit

    and [<AllowNullLiteral>] Annotation =
        abstract row: float with get, set
        abstract column: float with get, set
        abstract text: string with get, set
        abstract ``type``: string with get, set

    and [<AllowNullLiteral>] TokenInfo =
        abstract value: string with get, set

    and [<AllowNullLiteral>] Position =
        abstract row: float with get, set
        abstract column: float with get, set

    and [<AllowNullLiteral>] KeyBinding =
        abstract setDefaultHandler: kb: obj -> unit
        abstract setKeyboardHandler: kb: obj -> unit
        abstract addKeyboardHandler: kb: obj * pos: obj -> unit
        abstract removeKeyboardHandler: kb: obj -> bool
        abstract getKeyboardHandler: unit -> obj
        abstract onCommandKey: e: obj * hashId: obj * keyCode: obj -> unit
        abstract onTextInput: text: obj -> unit

    and [<AllowNullLiteral>] TextMode =
        abstract getTokenizer: unit -> obj
        abstract toggleCommentLines: state: obj * doc: obj * startRow: obj * endRow: obj -> unit
        abstract getNextLineIndent: state: obj * line: obj * tab: obj -> string
        abstract checkOutdent: state: obj * line: obj * input: obj -> bool
        abstract autoOutdent: state: obj * doc: obj * row: obj -> unit
        abstract createWorker: session: obj -> obj
        abstract createModeDelegates: mapping: obj -> unit
        abstract transformAction: state: obj * action: obj * editor: obj * session: obj * param: obj -> obj

    and [<AllowNullLiteral>] Ace =
        abstract require: moduleName: string -> obj
        abstract edit: el: string -> Editor
        abstract edit: el: HTMLElement -> Editor
        abstract createEditSession: text: Document * mode: TextMode -> IEditSession
        abstract createEditSession: text: string * mode: TextMode -> IEditSession

    and [<AllowNullLiteral>] Anchor =
        abstract on: ``event``: string * fn: Func<obj, obj> -> unit
        abstract getPosition: unit -> Position
        abstract getDocument: unit -> Document
        abstract onChange: e: obj -> unit
        abstract setPosition: row: float * column: float * noClip: bool -> unit
        abstract detach: unit -> unit

    and [<AllowNullLiteral>] AnchorType =
        [<Emit("new $0($1...)")>] abstract Create: doc: Document * row: float * column: float -> Anchor

    and [<AllowNullLiteral>] BackgroundTokenizer =
        abstract states: ResizeArray<obj> with get, set
        abstract setTokenizer: tokenizer: Tokenizer -> unit
        abstract setDocument: doc: Document -> unit
        abstract fireUpdateEvent: firstRow: float * lastRow: float -> unit
        abstract start: startRow: float -> unit
        abstract stop: unit -> unit
        abstract getTokens: row: float -> ResizeArray<TokenInfo>
        abstract getState: row: float -> string

    and [<AllowNullLiteral>] BackgroundTokenizerType =
        [<Emit("new $0($1...)")>] abstract Create: tokenizer: Tokenizer * editor: Editor -> BackgroundTokenizer

    and [<AllowNullLiteral>] Document =
        abstract on: ``event``: string * fn: Func<obj, obj> -> unit
        abstract setValue: text: string -> unit
        abstract getValue: unit -> string
        abstract createAnchor: row: float * column: float -> unit
        abstract getNewLineCharacter: unit -> string
        abstract setNewLineMode: newLineMode: string -> unit
        abstract getNewLineMode: unit -> string
        abstract isNewLine: text: string -> bool
        abstract getLine: row: float -> string
        abstract getLines: firstRow: float * lastRow: float -> ResizeArray<string>
        abstract getAllLines: unit -> ResizeArray<string>
        abstract getLength: unit -> float
        abstract getTextRange: range: Range -> string
        abstract insert: position: Position * text: string -> obj
        abstract insertLines: row: float * lines: ResizeArray<string> -> obj
        abstract insertNewLine: position: Position -> obj
        abstract insertInLine: position: obj * text: string -> obj
        abstract remove: range: Range -> obj
        abstract removeInLine: row: float * startColumn: float * endColumn: float -> obj
        abstract removeLines: firstRow: float * lastRow: float -> ResizeArray<string>
        abstract removeNewLine: row: float -> unit
        abstract replace: range: Range * text: string -> obj
        abstract applyDeltas: deltas: ResizeArray<Delta> -> unit
        abstract revertDeltas: deltas: ResizeArray<Delta> -> unit
        abstract indexToPosition: index: float * startRow: float -> Position
        abstract positionToIndex: pos: Position * startRow: float -> float

    and [<AllowNullLiteral>] DocumentType =
        [<Emit("new $0($1...)")>] abstract Create: ?text: string -> Document
        [<Emit("new $0($1...)")>] abstract Create: ?text: ResizeArray<string> -> Document

    and [<AllowNullLiteral>] IEditSession =
        abstract selection: Selection with get, set
        abstract bgTokenizer: BackgroundTokenizer with get, set
        abstract doc: Document with get, set
        abstract on: ``event``: string * fn: Func<obj, obj> -> unit
        abstract findMatchingBracket: position: Position -> unit
        abstract addFold: text: string * range: Range -> unit
        abstract getFoldAt: row: float * column: float -> obj
        abstract removeFold: arg: obj -> unit
        abstract expandFold: arg: obj -> unit
        abstract unfold: arg1: obj * arg2: bool -> unit
        abstract screenToDocumentColumn: row: float * column: float -> unit
        abstract getFoldDisplayLine: foldLine: obj * docRow: float * docColumn: float -> obj
        abstract getFoldsInRange: range: Range -> obj
        abstract highlight: text: string -> unit
        abstract setDocument: doc: Document -> unit
        abstract getDocument: unit -> Document
        abstract ``$resetRowCache``: row: float -> unit
        abstract setValue: text: string -> unit
        abstract setMode: mode: string -> unit
        abstract getValue: unit -> string
        abstract getSelection: unit -> Selection
        abstract getState: row: float -> string
        abstract getTokens: row: float -> ResizeArray<TokenInfo>
        abstract getTokenAt: row: float * column: float -> TokenInfo
        abstract setUndoManager: undoManager: UndoManager -> unit
        abstract getUndoManager: unit -> UndoManager
        abstract getTabString: unit -> string
        abstract setUseSoftTabs: useSoftTabs: bool -> unit
        abstract getUseSoftTabs: unit -> bool
        abstract setTabSize: tabSize: float -> unit
        abstract getTabSize: unit -> float
        abstract isTabStop: position: obj -> bool
        abstract setOverwrite: overwrite: bool -> unit
        abstract getOverwrite: unit -> bool
        abstract toggleOverwrite: unit -> unit
        abstract addGutterDecoration: row: float * className: string -> unit
        abstract removeGutterDecoration: row: float * className: string -> unit
        abstract getBreakpoints: unit -> ResizeArray<float>
        abstract setBreakpoints: rows: ResizeArray<obj> -> unit
        abstract clearBreakpoints: unit -> unit
        abstract setBreakpoint: row: float * className: string -> unit
        abstract clearBreakpoint: row: float -> unit
        abstract addMarker: range: Range * clazz: string * ``type``: Function * inFront: bool -> float
        abstract addMarker: range: Range * clazz: string * ``type``: string * inFront: bool -> float
        abstract addDynamicMarker: marker: obj * inFront: bool -> unit
        abstract removeMarker: markerId: float -> unit
        abstract getMarkers: inFront: bool -> ResizeArray<obj>
        abstract setAnnotations: annotations: ResizeArray<Annotation> -> unit
        abstract getAnnotations: unit -> obj
        abstract clearAnnotations: unit -> unit
        abstract ``$detectNewLine``: text: string -> unit
        abstract getWordRange: row: float * column: float -> Range
        abstract getAWordRange: row: float * column: float -> obj
        abstract setNewLineMode: newLineMode: string -> unit
        abstract getNewLineMode: unit -> string
        abstract setUseWorker: useWorker: bool -> unit
        abstract getUseWorker: unit -> bool
        abstract onReloadTokenizer: unit -> unit
        abstract ``$mode``: mode: TextMode -> unit
        abstract getMode: unit -> TextMode
        abstract setScrollTop: scrollTop: float -> unit
        abstract getScrollTop: unit -> float
        abstract setScrollLeft: unit -> unit
        abstract getScrollLeft: unit -> float
        abstract getScreenWidth: unit -> float
        abstract getLine: row: float -> string
        abstract getLines: firstRow: float * lastRow: float -> ResizeArray<string>
        abstract getLength: unit -> float
        abstract getTextRange: range: Range -> string
        abstract insert: position: Position * text: string -> obj
        abstract remove: range: Range -> obj
        abstract undoChanges: deltas: ResizeArray<obj> * dontSelect: bool -> Range
        abstract redoChanges: deltas: ResizeArray<obj> * dontSelect: bool -> Range
        abstract setUndoSelect: enable: bool -> unit
        abstract replace: range: Range * text: string -> obj
        abstract moveText: fromRange: Range * toPosition: obj -> Range
        abstract indentRows: startRow: float * endRow: float * indentString: string -> unit
        abstract outdentRows: range: Range -> unit
        abstract moveLinesUp: firstRow: float * lastRow: float -> float
        abstract moveLinesDown: firstRow: float * lastRow: float -> float
        abstract duplicateLines: firstRow: float * lastRow: float -> float
        abstract setUseWrapMode: useWrapMode: bool -> unit
        abstract getUseWrapMode: unit -> bool
        abstract setWrapLimitRange: min: float * max: float -> unit
        abstract adjustWrapLimit: desiredLimit: float -> bool
        abstract getWrapLimit: unit -> float
        abstract getWrapLimitRange: unit -> obj
        abstract ``$getDisplayTokens``: str: string * offset: float -> unit
        abstract ``$getStringScreenWidth``: str: string * maxScreenColumn: float * screenColumn: float -> ResizeArray<float>
        abstract getRowLength: row: float -> float
        abstract getScreenLastRowColumn: screenRow: float -> float
        abstract getDocumentLastRowColumn: docRow: float * docColumn: float -> float
        abstract getDocumentLastRowColumnPosition: docRow: float * docColumn: float -> float
        abstract getRowSplitData: unit -> string
        abstract getScreenTabSize: screenColumn: float -> float
        abstract screenToDocumentPosition: screenRow: float * screenColumn: float -> obj
        abstract documentToScreenPosition: docRow: float * docColumn: float -> obj
        abstract documentToScreenColumn: row: float * docColumn: float -> float
        abstract documentToScreenRow: docRow: float * docColumn: float -> unit
        abstract getScreenLength: unit -> float

    and [<AllowNullLiteral>] EditSessionType =
        [<Emit("new $0($1...)")>] abstract Create: text: string * ?mode: TextMode -> IEditSession
        [<Emit("new $0($1...)")>] abstract Create: content: string * ?mode: string -> IEditSession
        [<Emit("new $0($1...)")>] abstract Create: text: ResizeArray<string> * ?mode: string -> IEditSession

    and [<AllowNullLiteral>] Editor =
        abstract inMultiSelectMode: bool with get, set
        abstract commands: CommandManager with get, set
        abstract session: IEditSession with get, set
        abstract selection: Selection with get, set
        abstract renderer: VirtualRenderer with get, set
        abstract keyBinding: KeyBinding with get, set
        abstract container: HTMLElement with get, set
        abstract ``$blockScrolling``: float with get, set
        abstract on: ev: string * callback: Func<obj, obj> -> unit
        [<Emit("$0.addEventListener('change',$1...)")>] abstract addEventListener_change: callback: Func<EditorChangeEvent, obj> -> unit
        abstract addEventListener: ev: string * callback: Function -> unit
        abstract selectMoreLines: n: float -> unit
        abstract onTextInput: text: string -> unit
        abstract onCommandKey: e: obj * hashId: obj * keyCode: obj -> unit
        abstract onSelectionChange: e: obj -> unit
        abstract onChangeMode: ?e: obj -> unit
        abstract execCommand: command: string * ?args: obj -> unit
        abstract setOption: optionName: obj * optionValue: obj -> unit
        abstract setOptions: keyValueTuples: obj -> unit
        abstract getOption: name: obj -> obj
        abstract getOptions: unit -> obj
        abstract setKeyboardHandler: keyboardHandler: string -> unit
        abstract getKeyboardHandler: unit -> string
        abstract setSession: session: IEditSession -> unit
        abstract getSession: unit -> IEditSession
        abstract setValue: ``val``: string * ?cursorPos: float -> string
        abstract getValue: unit -> string
        abstract getSelection: unit -> Selection
        abstract resize: ?force: bool -> unit
        abstract setTheme: theme: string -> unit
        abstract getTheme: unit -> string
        abstract setStyle: style: string -> unit
        abstract unsetStyle: unit -> unit
        abstract setFontSize: size: string -> unit
        abstract focus: unit -> unit
        abstract isFocused: unit -> unit
        abstract blur: unit -> unit
        abstract onFocus: unit -> unit
        abstract onBlur: unit -> unit
        abstract onDocumentChange: e: obj -> unit
        abstract onCursorChange: unit -> unit
        abstract getCopyText: unit -> string
        abstract onCopy: unit -> unit
        abstract onCut: unit -> unit
        abstract onPaste: text: string -> unit
        abstract insert: text: string -> unit
        abstract setOverwrite: overwrite: bool -> unit
        abstract getOverwrite: unit -> bool
        abstract toggleOverwrite: unit -> unit
        abstract setScrollSpeed: speed: float -> unit
        abstract getScrollSpeed: unit -> float
        abstract setDragDelay: dragDelay: float -> unit
        abstract getDragDelay: unit -> float
        abstract setSelectionStyle: style: string -> unit
        abstract getSelectionStyle: unit -> string
        abstract setHighlightActiveLine: shouldHighlight: bool -> unit
        abstract getHighlightActiveLine: unit -> unit
        abstract setHighlightSelectedWord: shouldHighlight: bool -> unit
        abstract getHighlightSelectedWord: unit -> bool
        abstract setShowInvisibles: showInvisibles: bool -> unit
        abstract getShowInvisibles: unit -> bool
        abstract setShowPrintMargin: showPrintMargin: bool -> unit
        abstract getShowPrintMargin: unit -> bool
        abstract setPrintMarginColumn: showPrintMargin: float -> unit
        abstract getPrintMarginColumn: unit -> float
        abstract setReadOnly: readOnly: bool -> unit
        abstract getReadOnly: unit -> bool
        abstract setBehavioursEnabled: enabled: bool -> unit
        abstract getBehavioursEnabled: unit -> bool
        abstract setWrapBehavioursEnabled: enabled: bool -> unit
        abstract getWrapBehavioursEnabled: unit -> unit
        abstract setShowFoldWidgets: show: bool -> unit
        abstract getShowFoldWidgets: unit -> unit
        abstract remove: dir: string -> unit
        abstract removeWordRight: unit -> unit
        abstract removeWordLeft: unit -> unit
        abstract removeToLineStart: unit -> unit
        abstract removeToLineEnd: unit -> unit
        abstract splitLine: unit -> unit
        abstract transposeLetters: unit -> unit
        abstract toLowerCase: unit -> unit
        abstract toUpperCase: unit -> unit
        abstract indent: unit -> unit
        abstract blockIndent: unit -> unit
        abstract blockOutdent: ?arg: string -> unit
        abstract toggleCommentLines: unit -> unit
        abstract getNumberAt: unit -> float
        abstract modifyNumber: amount: float -> unit
        abstract removeLines: unit -> unit
        abstract moveLinesDown: unit -> float
        abstract moveLinesUp: unit -> float
        abstract moveText: fromRange: Range * toPosition: obj -> Range
        abstract copyLinesUp: unit -> float
        abstract copyLinesDown: unit -> float
        abstract getFirstVisibleRow: unit -> float
        abstract getLastVisibleRow: unit -> float
        abstract isRowVisible: row: float -> bool
        abstract isRowFullyVisible: row: float -> bool
        abstract selectPageDown: unit -> unit
        abstract selectPageUp: unit -> unit
        abstract gotoPageDown: unit -> unit
        abstract gotoPageUp: unit -> unit
        abstract scrollPageDown: unit -> unit
        abstract scrollPageUp: unit -> unit
        abstract scrollToRow: unit -> unit
        abstract scrollToLine: line: float * center: bool * animate: bool * callback: Function -> unit
        abstract centerSelection: unit -> unit
        abstract getCursorPosition: unit -> Position
        abstract getCursorPositionScreen: unit -> float
        abstract getSelectionRange: unit -> Range
        abstract selectAll: unit -> unit
        abstract clearSelection: unit -> unit
        abstract moveCursorTo: row: float * ?column: float * ?animate: bool -> unit
        abstract moveCursorToPosition: position: Position -> unit
        abstract jumpToMatching: unit -> unit
        abstract gotoLine: lineNumber: float * ?column: float * ?animate: bool -> unit
        abstract navigateTo: row: float * column: float -> unit
        abstract navigateUp: ?times: float -> unit
        abstract navigateDown: ?times: float -> unit
        abstract navigateLeft: ?times: float -> unit
        abstract navigateRight: times: float -> unit
        abstract navigateLineStart: unit -> unit
        abstract navigateLineEnd: unit -> unit
        abstract navigateFileEnd: unit -> unit
        abstract navigateFileStart: unit -> unit
        abstract navigateWordRight: unit -> unit
        abstract navigateWordLeft: unit -> unit
        abstract replace: replacement: string * ?options: obj -> unit
        abstract replaceAll: replacement: string * ?options: obj -> unit
        abstract getLastSearchOptions: unit -> obj
        abstract find: needle: string * ?options: obj * ?animate: bool -> unit
        abstract findNext: ?options: obj * ?animate: bool -> unit
        abstract findPrevious: ?options: obj * ?animate: bool -> unit
        abstract undo: unit -> unit
        abstract redo: unit -> unit
        abstract destroy: unit -> unit

    and [<AllowNullLiteral>] EditorType =
        [<Emit("new $0($1...)")>] abstract Create: renderer: VirtualRenderer * ?session: IEditSession -> Editor

    and [<AllowNullLiteral>] EditorChangeEvent =
        abstract start: Position with get, set
        abstract ``end``: Position with get, set
        abstract action: string with get, set
        abstract lines: ResizeArray<obj> with get, set

    and [<AllowNullLiteral>] PlaceHolder =
        abstract on: ``event``: string * fn: Func<obj, obj> -> unit
        abstract setup: unit -> unit
        abstract showOtherMarkers: unit -> unit
        abstract hideOtherMarkers: unit -> unit
        abstract onUpdate: unit -> unit
        abstract onCursorChange: unit -> unit
        abstract detach: unit -> unit
        abstract cancel: unit -> unit

    and [<AllowNullLiteral>] PlaceHolderType =
        [<Emit("new $0($1...)")>] abstract Create: session: Document * length: float * pos: float * others: string * mainClass: string * othersClass: string -> PlaceHolder
        [<Emit("new $0($1...)")>] abstract Create: session: IEditSession * length: float * pos: Position * positions: ResizeArray<Position> -> PlaceHolder

    and [<AllowNullLiteral>] IRangeList =
        abstract ranges: ResizeArray<Range> with get, set
        abstract pointIndex: pos: Position * ?startIndex: float -> unit
        abstract addList: ranges: ResizeArray<Range> -> unit
        abstract add: ranges: Range -> unit
        abstract merge: unit -> ResizeArray<Range>
        abstract substractPoint: pos: Position -> unit

    and [<AllowNullLiteral>] RangeListType =
        [<Emit("new $0($1...)")>] abstract Create: unit -> IRangeList

    and [<AllowNullLiteral>] Range =
        abstract startRow: float with get, set
        abstract startColumn: float with get, set
        abstract endRow: float with get, set
        abstract endColumn: float with get, set
        abstract start: Position with get, set
        abstract ``end``: Position with get, set
        abstract isEmpty: unit -> bool
        abstract isEqual: range: Range -> unit
        abstract toString: unit -> unit
        abstract contains: row: float * column: float -> bool
        abstract compareRange: range: Range -> float
        abstract comparePoint: p: Range -> float
        abstract containsRange: range: Range -> bool
        abstract intersects: range: Range -> bool
        abstract isEnd: row: float * column: float -> bool
        abstract isStart: row: float * column: float -> bool
        abstract setStart: row: float * column: float -> unit
        abstract setEnd: row: float * column: float -> unit
        abstract inside: row: float * column: float -> bool
        abstract insideStart: row: float * column: float -> bool
        abstract insideEnd: row: float * column: float -> bool
        abstract compare: row: float * column: float -> float
        abstract compareStart: row: float * column: float -> float
        abstract compareEnd: row: float * column: float -> float
        abstract compareInside: row: float * column: float -> float
        abstract clipRows: firstRow: float * lastRow: float -> Range
        abstract extend: row: float * column: float -> Range
        abstract isMultiLine: unit -> bool
        abstract clone: unit -> Range
        abstract collapseRows: unit -> Range
        abstract toScreenRange: session: IEditSession -> Range
        abstract fromPoints: start: Range * ``end``: Range -> Range

    and [<AllowNullLiteral>] RangeType =
        abstract fromPoints: pos1: Position * pos2: Position -> Range
        [<Emit("new $0($1...)")>] abstract Create: startRow: float * startColumn: float * endRow: float * endColumn: float -> Range

    and [<AllowNullLiteral>] RenderLoop =
        interface end

    and [<AllowNullLiteral>] RenderLoopType =
        [<Emit("new $0($1...)")>] abstract Create: unit -> RenderLoop

    and [<AllowNullLiteral>] ScrollBar =
        abstract onScroll: e: obj -> unit
        abstract getWidth: unit -> float
        abstract setHeight: height: float -> unit
        abstract setInnerHeight: height: float -> unit
        abstract setScrollTop: scrollTop: float -> unit

    and [<AllowNullLiteral>] ScrollBarType =
        [<Emit("new $0($1...)")>] abstract Create: parent: HTMLElement -> ScrollBar

    and [<AllowNullLiteral>] Search =
        abstract set: options: obj -> Search
        abstract getOptions: unit -> obj
        abstract setOptions: An: obj -> unit
        abstract find: session: IEditSession -> Range
        abstract findAll: session: IEditSession -> ResizeArray<Range>
        abstract replace: input: string * replacement: string -> string

    and [<AllowNullLiteral>] SearchType =
        [<Emit("new $0($1...)")>] abstract Create: unit -> Search

    and [<AllowNullLiteral>] Selection =
        abstract addEventListener: ev: string * callback: Function -> unit
        abstract moveCursorWordLeft: unit -> unit
        abstract moveCursorWordRight: unit -> unit
        abstract fromOrientedRange: range: Range -> unit
        abstract setSelectionRange: ``match``: obj -> unit
        abstract getAllRanges: unit -> ResizeArray<Range>
        abstract on: ``event``: string * fn: Func<obj, obj> -> unit
        abstract addRange: range: Range -> unit
        abstract isEmpty: unit -> bool
        abstract isMultiLine: unit -> bool
        abstract getCursor: unit -> Position
        abstract setSelectionAnchor: row: float * column: float -> unit
        abstract getSelectionAnchor: unit -> obj
        abstract getSelectionLead: unit -> obj
        abstract shiftSelection: columns: float -> unit
        abstract isBackwards: unit -> bool
        abstract getRange: unit -> Range
        abstract clearSelection: unit -> unit
        abstract selectAll: unit -> unit
        abstract setRange: range: Range * reverse: bool -> unit
        abstract selectTo: row: float * column: float -> unit
        abstract selectToPosition: pos: obj -> unit
        abstract selectUp: unit -> unit
        abstract selectDown: unit -> unit
        abstract selectRight: unit -> unit
        abstract selectLeft: unit -> unit
        abstract selectLineStart: unit -> unit
        abstract selectLineEnd: unit -> unit
        abstract selectFileEnd: unit -> unit
        abstract selectFileStart: unit -> unit
        abstract selectWordRight: unit -> unit
        abstract selectWordLeft: unit -> unit
        abstract getWordRange: unit -> unit
        abstract selectWord: unit -> unit
        abstract selectAWord: unit -> unit
        abstract selectLine: unit -> unit
        abstract moveCursorUp: unit -> unit
        abstract moveCursorDown: unit -> unit
        abstract moveCursorLeft: unit -> unit
        abstract moveCursorRight: unit -> unit
        abstract moveCursorLineStart: unit -> unit
        abstract moveCursorLineEnd: unit -> unit
        abstract moveCursorFileEnd: unit -> unit
        abstract moveCursorFileStart: unit -> unit
        abstract moveCursorLongWordRight: unit -> unit
        abstract moveCursorLongWordLeft: unit -> unit
        abstract moveCursorBy: rows: float * chars: float -> unit
        abstract moveCursorToPosition: position: obj -> unit
        abstract moveCursorTo: row: float * column: float * ?keepDesiredColumn: bool -> unit
        abstract moveCursorToScreen: row: float * column: float * keepDesiredColumn: bool -> unit

    and [<AllowNullLiteral>] SelectionType =
        [<Emit("new $0($1...)")>] abstract Create: session: IEditSession -> Selection

    and [<AllowNullLiteral>] Split =
        abstract getSplits: unit -> float
        abstract getEditor: idx: float -> unit
        abstract getCurrentEditor: unit -> Editor
        abstract focus: unit -> unit
        abstract blur: unit -> unit
        abstract setTheme: theme: string -> unit
        abstract setKeyboardHandler: keybinding: string -> unit
        abstract forEach: callback: Function * scope: string -> unit
        abstract setFontSize: size: float -> unit
        abstract setSession: session: IEditSession * idx: float -> unit
        abstract getOrientation: unit -> float
        abstract setOrientation: orientation: float -> unit
        abstract resize: unit -> unit

    and [<AllowNullLiteral>] SplitType =
        [<Emit("new $0($1...)")>] abstract Create: unit -> Split

    and [<AllowNullLiteral>] TokenIterator =
        abstract stepBackward: unit -> ResizeArray<string>
        abstract stepForward: unit -> string
        abstract getCurrentToken: unit -> TokenInfo
        abstract getCurrentTokenRow: unit -> float
        abstract getCurrentTokenColumn: unit -> float

    and [<AllowNullLiteral>] TokenIteratorType =
        [<Emit("new $0($1...)")>] abstract Create: session: IEditSession * initialRow: float * initialColumn: float -> TokenIterator

    and [<AllowNullLiteral>] Tokenizer =
        abstract getLineTokens: unit -> obj

    and [<AllowNullLiteral>] TokenizerType =
        [<Emit("new $0($1...)")>] abstract Create: rules: obj * flag: string -> Tokenizer

    and [<AllowNullLiteral>] UndoManager =
        abstract execute: options: obj -> unit
        abstract undo: ?dontSelect: bool -> Range
        abstract redo: dontSelect: bool -> unit
        abstract reset: unit -> unit
        abstract hasUndo: unit -> bool
        abstract hasRedo: unit -> bool
        abstract isClean: unit -> bool
        abstract markClean: unit -> unit

    and [<AllowNullLiteral>] UndoManagerType =
        [<Emit("new $0($1...)")>] abstract Create: unit -> UndoManager

    and [<AllowNullLiteral>] VirtualRenderer =
        abstract scroller: obj with get, set
        abstract characterWidth: float with get, set
        abstract lineHeight: float with get, set
        abstract screenToTextCoordinates: left: float * top: float -> unit
        abstract setSession: session: IEditSession -> unit
        abstract updateLines: firstRow: float * lastRow: float -> unit
        abstract updateText: unit -> unit
        abstract updateFull: force: bool -> unit
        abstract updateFontSize: unit -> unit
        abstract onResize: force: bool * gutterWidth: float * width: float * height: float -> unit
        abstract adjustWrapLimit: unit -> unit
        abstract setAnimatedScroll: shouldAnimate: bool -> unit
        abstract getAnimatedScroll: unit -> bool
        abstract setShowInvisibles: showInvisibles: bool -> unit
        abstract getShowInvisibles: unit -> bool
        abstract setShowPrintMargin: showPrintMargin: bool -> unit
        abstract getShowPrintMargin: unit -> bool
        abstract setPrintMarginColumn: showPrintMargin: bool -> unit
        abstract getPrintMarginColumn: unit -> bool
        abstract getShowGutter: unit -> bool
        abstract setShowGutter: show: bool -> unit
        abstract getContainerElement: unit -> HTMLElement
        abstract getMouseEventTarget: unit -> HTMLElement
        abstract getTextAreaContainer: unit -> HTMLElement
        abstract getFirstVisibleRow: unit -> float
        abstract getFirstFullyVisibleRow: unit -> float
        abstract getLastFullyVisibleRow: unit -> float
        abstract getLastVisibleRow: unit -> float
        abstract setPadding: padding: float -> unit
        abstract getHScrollBarAlwaysVisible: unit -> bool
        abstract setHScrollBarAlwaysVisible: alwaysVisible: bool -> unit
        abstract updateFrontMarkers: unit -> unit
        abstract updateBackMarkers: unit -> unit
        abstract addGutterDecoration: unit -> unit
        abstract removeGutterDecoration: unit -> unit
        abstract updateBreakpoints: unit -> unit
        abstract setAnnotations: annotations: ResizeArray<obj> -> unit
        abstract updateCursor: unit -> unit
        abstract hideCursor: unit -> unit
        abstract showCursor: unit -> unit
        abstract scrollCursorIntoView: unit -> unit
        abstract getScrollTop: unit -> float
        abstract getScrollLeft: unit -> float
        abstract getScrollTopRow: unit -> float
        abstract getScrollBottomRow: unit -> float
        abstract scrollToRow: row: float -> unit
        abstract scrollToLine: line: float * center: bool * animate: bool * callback: Function -> unit
        abstract scrollToY: scrollTop: float -> float
        abstract scrollToX: scrollLeft: float -> float
        abstract scrollBy: deltaX: float * deltaY: float -> unit
        abstract isScrollableBy: deltaX: float * deltaY: float -> bool
        abstract textToScreenCoordinates: row: float * column: float -> obj
        abstract visualizeFocus: unit -> unit
        abstract visualizeBlur: unit -> unit
        abstract showComposition: position: float -> unit
        abstract setCompositionText: text: string -> unit
        abstract hideComposition: unit -> unit
        abstract setTheme: theme: string -> unit
        abstract getTheme: unit -> string
        abstract setStyle: style: string -> unit
        abstract unsetStyle: style: string -> unit
        abstract destroy: unit -> unit

    and [<AllowNullLiteral>] VirtualRendererType =
        [<Emit("new $0($1...)")>] abstract Create: container: HTMLElement * ?theme: string -> VirtualRenderer

    type [<Import("*","AceAjax")>] Globals =
        static member Anchor with get(): AnchorType = jsNative and set(v: AnchorType): unit = jsNative
        static member BackgroundTokenizer with get(): BackgroundTokenizerType = jsNative and set(v: BackgroundTokenizerType): unit = jsNative
        static member Document with get(): DocumentType = jsNative and set(v: DocumentType): unit = jsNative
        static member EditSession with get(): EditSessionType = jsNative and set(v: EditSessionType): unit = jsNative
        static member Editor with get(): EditorType = jsNative and set(v: EditorType): unit = jsNative
        static member PlaceHolder with get(): PlaceHolderType = jsNative and set(v: PlaceHolderType): unit = jsNative
        static member RangeList with get(): RangeListType = jsNative and set(v: RangeListType): unit = jsNative
        static member Range with get(): RangeType = jsNative and set(v: RangeType): unit = jsNative
        static member RenderLoop with get(): RenderLoopType = jsNative and set(v: RenderLoopType): unit = jsNative
        static member ScrollBar with get(): ScrollBarType = jsNative and set(v: ScrollBarType): unit = jsNative
        static member Search with get(): SearchType = jsNative and set(v: SearchType): unit = jsNative
        static member Selection with get(): SelectionType = jsNative and set(v: SelectionType): unit = jsNative
        static member Split with get(): SplitType = jsNative and set(v: SplitType): unit = jsNative
        static member TokenIterator with get(): TokenIteratorType = jsNative and set(v: TokenIteratorType): unit = jsNative
        static member Tokenizer with get(): TokenizerType = jsNative and set(v: TokenizerType): unit = jsNative
        static member UndoManager with get(): UndoManagerType = jsNative and set(v: UndoManagerType): unit = jsNative
        static member VirtualRenderer with get(): VirtualRendererType = jsNative and set(v: VirtualRendererType): unit = jsNative
        
type [<Erase>]Globals =
    [<Global>] static member ace with get(): AceAjax.Ace = jsNative and set(v: AceAjax.Ace): unit = jsNative