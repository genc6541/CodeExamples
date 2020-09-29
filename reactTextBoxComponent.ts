import * as React from "react";
import ReactDOM from "react-dom";
import { ReactNode } from "react";
import { Input, InputNumber, AutoComplete, Button } from "antd";
import Search from "antd/lib/input/Search";
import TextArea from "antd/lib/input/TextArea";
import { DataEntryControl, DataEntryControlState, DataEntryControlProps } from '../../../base/data-entry-control';
import { InputNumberOptions } from "./textbox-inputnumber-options";
import { TextBoxComponentType } from "./textbox-enums";
import { SearchOptions } from "./textbox-search-options";
import { InputOptions } from "./textbox-input-options";
import { TextBoxConstants } from "./textbox-constants";
import { TextAreaOptions } from "./textbox-textarea-options";
import { AutoCompleteOptions } from "./textbox-autocomplete-options";
import { TextBoxSearchHandlerView, ControlOnPressEnterHandlerView, ControlOnKeyDownHandlerView, ControlOnKeyUpHandlerView, ControlOnKeyPressHandlerView, ControlOnBlurHandlerView, CommonDataEntryEventArguments, ControlChangeHandlerView } from "../../../model/view/control-change-handler-view";
import { EL } from 'surface-helper/src/helpers/expression-language';
import { isNumeric, isFloat, formatMoneyOnChange, parseMoneyOnChange, manageZeroDecimalValues } from 'surface-helper/src/helpers/number-helper';
import { isString } from 'surface-helper/src/helpers/string-helper';
import { surfaceForm, surfaceContext } from "../../../base/hoc/hoc";
import { getMultiLanguageValueForPlaceholder } from 'surface-helper/src/helpers/multi-language-helper';
import { isUndefined, isEqual } from 'lodash';
import './textbox-css.css';
import { TextboxDetailSearch } from "./textbox-detail-search";
import { DetailSearchOptions } from "./textbox-detail-search-options";

export interface TextBoxProps extends DataEntryControlProps {
    options?: SearchOptions | DetailSearchOptions | InputNumberOptions | InputOptions | AutoCompleteOptions;
    size?: "small" | "medium" | "large";
    componentType?: TextBoxComponentType;
    placeHolderMessage?: string;
    allowStartWithZero?: boolean;
    className?: string;
    disabledChars?: string[];
    tabIndex?: number;
    customWidth?: string;
    moneyDecimal?: number;
    suffix?: string;
    suffixIcon?: string;
    suffixText?: string;
    suffixIfEmpty?: boolean;
    isInputRight?: boolean;
    onlyFloat?: boolean;
    onlyMoney?: boolean;
    onlyString?: boolean;
    onlyNumber?: boolean;
    onSuffixClick?: () => void;
    onPressEnter?: (e: ControlOnPressEnterHandlerView) => void;
    onFocusOut?: (e: any) => void;
    onSearch?: (e: TextBoxSearchHandlerView) => void;
    onKeyDown?: (e: ControlOnKeyDownHandlerView) => void;
    onKeyUp?: (e: ControlOnKeyUpHandlerView) => void;
    onKeyPress?: (e: ControlOnKeyPressHandlerView) => boolean;
    onBlur?: (e: ControlOnBlurHandlerView) => void;
    formatter?: (value: any) => any;
    parser?: (value: any) => any;
    isUsageInDataGrid ?: boolean;
}
export interface TextBoxState extends DataEntryControlState {
    suffixIfEmpty?: boolean;
    isSearchPopupVisible: boolean;
    detailSearchSelectedValue: any;
}
class TextBox extends DataEntryControl<TextBoxProps, TextBoxState> {
    isDetailSearchActive: boolean = false;
    static defaultProps = {
        componentType: TextBoxComponentType.Input,
        visible: TextBoxConstants.DefaultTextBoxVisible,
        disabled: TextBoxConstants.DefaultTextBoxDisabled,
        isStyleActive: TextBoxConstants.DefaultTextBoxIsStyleActive,
    };
    constructor(props: any) {
        super(props);
        this.setDefaultValue(this.handleGetDefaultValue(), this.isComponentHasBindingPathProp());
        this.state = { suffixIfEmpty: !this.props.suffixIfEmpty, isSearchPopupVisible: false, detailSearchSelectedValue: "" };
    }

    checkInputOptionsRegex = (value: any) => {
        let inputOptions = this.getOptions() as InputOptions;
        if (inputOptions.regex) {
            let exp = "RegexMatch('" + value + "','" + inputOptions.regex + "')";
            let evaluateResult = new EL()._evaluate(exp, null, null);
            if (evaluateResult === true) {
                let newValue = "";
                this.setFormDataField(newValue, this.getBindingPath());
                value = newValue;
            }
            else {
                this.setFormDataField(value, this.getBindingPath());
            }
        }
        return value;
    }
    // #endregion
    // #region (GET METHODS)
    getComponentType() {
        return "textbox";
    }
    getOptions = () => {
        if (!isUndefined(this.props.options)) {
            return this.props.options;
        }
        else if (this.props.componentType === TextBoxComponentType.Input) {
            return new InputOptions({});
        }
        else if (this.props.componentType === TextBoxComponentType.NumericInput) {
            return new InputOptions({});
        }
        else if (this.props.componentType === TextBoxComponentType.Password) {
            return new InputOptions({});
        }
        else if (this.props.componentType === TextBoxComponentType.Search) {
            return new SearchOptions({});
        }
        else if (this.props.componentType === TextBoxComponentType.DetailSearch) {
            return new DetailSearchOptions({});
        }
        else if (this.props.componentType === TextBoxComponentType.InputNumber) {
            return new InputNumberOptions({});
        }
        else if (this.props.componentType === TextBoxComponentType.TextArea) {
            return new TextAreaOptions({});
        }
        else if (this.props.componentType === TextBoxComponentType.AutoComplete) {
            return new AutoCompleteOptions({});
        }
        return new InputOptions({});
    }
    getCommonProps = () => {
        let commonProps: any = {};
        let tabIndex = this.props.tabIndex;
        let classNameControl = this.props.isInputRight ? (this.props.className + " text-aligner") : this.props.className;
        this.props.size ? (classNameControl += " input-text-box-" + this.props.size) : (classNameControl = classNameControl);
        commonProps = {
            style: { width: this.props.customWidth ? this.props.customWidth : "100%" },
            placeholder: getMultiLanguageValueForPlaceholder(this.props.placeHolder, TextBoxConstants.DefaultTextBoxPlaceHolder, this.props.disabled, this.props.screen!.screencode),
            className: classNameControl,
            tabIndex: tabIndex,
            autoComplete: "off"
        };
        return commonProps;
    }
    getRenderMetod(): ReactNode {
        let commonProps = this.getCommonProps();
        switch (this.props.componentType) {
            case TextBoxComponentType.Input:
                return this.renderInput(commonProps);
            case TextBoxComponentType.InputNumber:
                return this.renderInputNumber(commonProps);
            case TextBoxComponentType.Search:
                return this.renderSearch(commonProps);
            case TextBoxComponentType.DetailSearch:
                return this.renderDetailSearch(commonProps);
            case TextBoxComponentType.TextArea:
                return this.renderTextArea(commonProps);
            case TextBoxComponentType.AutoComplete:
                return this.renderAutoComplete(commonProps);
            case TextBoxComponentType.Password:
                return this.renderPassword(commonProps);
            default:
                return this.renderInput(commonProps);
        }
    }
    getValueProp = () => {
        let value = undefined;
        if (!this.isComponentHasBindingPathProp()) {
            if (!isUndefined(this.props.value)) {
                value = { value: this.props.value };
            }
        } else {
            value = { value: this.getFormDataField(this.getBindingPath()) };
        }
        return value;
    }

    getValuePropForDetailSearch = () => {
        let value = undefined;
        if (this.props.componentType === TextBoxComponentType.DetailSearch) {

            if (!this.isComponentHasBindingPathProp()) {
                if (this.isDetailSearchActive) {
                    if (this.state.detailSearchSelectedValue) {
                        value = { value: this.state.detailSearchSelectedValue };
                    }
                } else {
                    if (!isUndefined(this.props.value)) {
                        value = { value: this.props.value };
                    }
                }


            } else {
                value = { value: this.getFormDataField(this.getBindingPath()) };
            }
            return value;
        }
    }

    getDefaultValueProp = () => {
        let defaultValue = this.handleGetDefaultValue();
        if (!defaultValue) defaultValue = this.props.defaultValue;
        let result = { defaultValue: defaultValue };
        // if (!this.isComponentHasBindingPathProp() && !isUndefined(defaultValue)) {
        //     result = { defaultValue: defaultValue };
        // }
        return result;
    }

    // #endregion
    // #region (CHANGE METHODS)
    onChangeInput = (e: any) => {
        let value = e[0].target.value;
        if (this.props.onlyString && !isString(value) && (value.length > 0)) {
            return;
        } else if (this.props.onlyNumber && value && !isNumeric(value) && (e.target.value.length > 0)) {
            return;
        }
        if (this.props.onChangeValue) {
            this.props.onChangeValue({ id: this.getId(), value: value, bindingPath: this.getBindingPath(), externalParameter: this.props.externalParameter } as CommonDataEntryEventArguments);
        }
        return value;
    }
    onChangeInputNumber = (e: any) => {
        let value = e[0];
        if (this.props.onChangeValue) {
            this.props.onChangeValue({ id: this.getId(), value: value, bindingPath: this.getBindingPath(), externalParameter: this.props.externalParameter } as CommonDataEntryEventArguments);
        }
        return value;
    }
    onChangeAutoComplete = (e: any) => {
        let value = e[0];
        if (this.props.onChangeValue) {
            this.props.onChangeValue({ id: this.getId(), value: value, bindingPath: this.getBindingPath(), externalParameter: this.props.externalParameter } as CommonDataEntryEventArguments);
        }
        return value;
    }
    onChangePassword = (e: any) => {
        let value = e[0].target.value;
        if (this.props.onChangeValue) {
            this.props.onChangeValue({ id: this.getId(), value: value, bindingPath: this.getBindingPath(), externalParameter: this.props.externalParameter } as CommonDataEntryEventArguments);
        }
        return value;
    }

    // IN UNCONTROLLED USAGE METHOD
    onChange = (e: any) => {
        if (this.isComponentHasBindingPathProp()) {
            return;
        }
        let value: any;
        switch (this.props.componentType) {
            case TextBoxComponentType.TextArea:
            case TextBoxComponentType.Search:
            case TextBoxComponentType.DetailSearch:
                value = e.target.value;
                break;
            case TextBoxComponentType.Input:
            case TextBoxComponentType.InputNumber:
                value = e;
                break;
            case TextBoxComponentType.AutoComplete:
                value = e[0];
                break;
            case TextBoxComponentType.Password:
                value = e[0].target.value;
                break;
        }
        if (this.props.onChangeValue) {
            this.props.onChangeValue({ id: this.getId(), value: value, externalParameter: this.props.externalParameter } as CommonDataEntryEventArguments);
        }
    }
    onBlurTrigger = (value: any) => {
        let controlOnBlurHandlerView: ControlOnBlurHandlerView = this.getCommonDataEntryEventArguments("handleOnBlur") as ControlOnBlurHandlerView;
        controlOnBlurHandlerView.bindingPath = this.getBindingPath();
        controlOnBlurHandlerView.id = this.getId();
        controlOnBlurHandlerView.eventName = "onBlur";
        controlOnBlurHandlerView.value = value;
        controlOnBlurHandlerView.externalParameter = this.props.externalParameter;
        if (!this.isComponentHasBindingPathProp() && value && this.props.parser) {
            controlOnBlurHandlerView.value = this.props.parser(value);
        }
        if (!this.isComponentHasBindingPathProp() && value && this.props.onlyMoney) {
            controlOnBlurHandlerView.value = manageZeroDecimalValues(parseMoneyOnChange(value, this.props.moneyDecimal), this.props.moneyDecimal);
        }
        if (this.props.onBlur) {
            this.props.onBlur(controlOnBlurHandlerView);
            if (this.isComponentHasBindingPathProp()) {
                value = this.getFormDataField(this.getBindingPath());
                if (this.props.onlyMoney) {
                    value = formatMoneyOnChange(value, this.props.moneyDecimal);
                }
            }
        }
        if (this.props.onChangeValue && !isEqual(this.props.isUsageInDataGrid, true)) {
            this.props.onChangeValue(controlOnBlurHandlerView);
        }
        if (value && this.props.parser) {
            value = this.props.parser(value);
        }
        if (value && this.props.onlyMoney) {
            value = manageZeroDecimalValues(parseMoneyOnChange(value, this.props.moneyDecimal), this.props.moneyDecimal);
        }
        // TODO:AS - Bu metodu incelemek lazým, setFormDataField'a neden ihtiyça var, zaten onchange'de formdata set ediliyor??
        if (!isEqual(this.props.isUsageInDataGrid, true)) {
            this.setFormDataField(value, this.getBindingPath());
        }
    }
    // #endregion
    // #region (HANDLE METHODS)
    handleOnBlur = (e: React.FocusEvent<HTMLInputElement>) => {
        let componentType = this.props.componentType;
        let value: any = "";
        let targetValue: any = this.getTargetValue(e);
        switch (componentType) {
            case TextBoxComponentType.Input:
                value = targetValue;
                value = this.checkInputOptionsRegex(value);
                break;
            case TextBoxComponentType.InputNumber:
                value = targetValue;
                break;
            case TextBoxComponentType.TextArea:
            case TextBoxComponentType.Search:
            case TextBoxComponentType.Password:
            case TextBoxComponentType.DetailSearch:
                value = targetValue;
                break;
            case TextBoxComponentType.AutoComplete:
                value = targetValue as any;
                break;
        }

        if (this.props.onlyFloat && !(isFloat(value) || isNumeric(value))) {
            let updatedValue = "";
            this.setFormDataField(updatedValue, this.getBindingPath());
            value = updatedValue;
        }
        if (this.props.onlyNumber) {
            if (!this.props.allowStartWithZero && value && value.startsWith("0")) {
                let replacedValue = value.replace(/^0+/, '');
                this.setFormDataField(replacedValue, this.getBindingPath());
                value = replacedValue;
            }
            let inputNumberOptions = this.getOptions() as InputNumberOptions;
            let numberValue = Number(value);
            if (numberValue > inputNumberOptions.maxValue!) {
                this.setFormDataField(inputNumberOptions.maxValue, this.getBindingPath());
                value = inputNumberOptions.maxValue;
            }
        }

        if (this.props.suffixIfEmpty) {
            if (value === "") {
                this.setState({ suffixIfEmpty: false });
            } else {
                this.setState({ suffixIfEmpty: true });
            }
        }
        this.onBlurTrigger(value);
    }
    handleSuffixButtonClick = () => {
        if (this.props.componentType === TextBoxComponentType.DetailSearch) {
            this.setState({ isSearchPopupVisible: true });
        }
        if (this.props.onSuffixClick) {
            this.props.onSuffixClick();
        }

    }
    getTargetValue = (e: React.FocusEvent<HTMLInputElement>) => {
        let value: any = "";

        if (e && typeof e === 'string') {
            value = e;
        }

        if (e && e.target && e.target.value) {
            value = e.target.value;
        }
        return value;
    }

    handleSeleniumUnique = () => {
        let node: any = ReactDOM.findDOMNode(this);
        let inputSelection = node.querySelectorAll("input");
        let textareaSelection = node.querySelectorAll("textarea");
        if (inputSelection.length > 0 || textareaSelection.length > 0) {
            if (node && this.props && this.props.screen && this.props.screen.screencode && this.getId()) {
                inputSelection[0] && inputSelection[0].classList.add("textbox-input-" + this.props.screen.screencode + "-" + this.getId());
                textareaSelection[0] && textareaSelection[0].classList.add("textbox-text-area-" + this.props.screen.screencode + "-" + this.getId());
            }
        }
    }
    handleGetDefaultValue = () => {
        let defaultValue: any;
        let componentType = this.props.componentType;
        switch (componentType) {
            case TextBoxComponentType.Input:
                let inputOptions = this.getOptions() as InputOptions;
                defaultValue = inputOptions.defaultValue ? inputOptions.defaultValue : TextBoxConstants.DefaultInputDefaultValue;
                break;
            case TextBoxComponentType.InputNumber:
                let inputNumberOptions = this.getOptions() as InputNumberOptions;
                defaultValue = inputNumberOptions.defaultValue ? inputNumberOptions.defaultValue : TextBoxConstants.DefaultInputNumberDefaultValue;
                break;
            case TextBoxComponentType.Password:
                let passwordOptions = this.getOptions() as InputOptions;
                defaultValue = passwordOptions.defaultValue ? passwordOptions.defaultValue : TextBoxConstants.DefaultPasswordDefaultValue;
                break;
            case TextBoxComponentType.Search:
                let searchOptions = this.getOptions() as SearchOptions;
                defaultValue = searchOptions.defaultValue ? searchOptions.defaultValue : TextBoxConstants.DefaultSearchDefaultValue;
                break;
            case TextBoxComponentType.NumericInput:
                let numericInputOptions = this.getOptions() as InputOptions;
                defaultValue = this.props.defaultValue ? numericInputOptions.defaultValue : TextBoxConstants.DefaultNumericInputDefaultValue;
                break;
            case TextBoxComponentType.AutoComplete:
                let autoCompleteOptions = this.getOptions() as AutoCompleteOptions;
                defaultValue = autoCompleteOptions.defaultValue ? autoCompleteOptions.defaultValue : TextBoxConstants.DefaultAutoCompleteDefaultValue;
                break;
            default:
                let defaultInputOptions = this.getOptions() as InputOptions;
                defaultValue = defaultInputOptions.defaultValue ? defaultInputOptions.defaultValue : TextBoxConstants.DefaultInputDefaultValue;
                break;
        }
        return defaultValue;
    }
    handleFormatNumber = (value: any) => {
        value += '';
        const list = value.split('.');
        const prefix = list[0].charAt(0) === '-' ? '-' : '';
        let num = prefix ? list[0].slice(1) : list[0];
        let result = '';
        while (num.length > 3) {
            result = `,${num.slice(-3)}${result}`;
            num = num.slice(0, num.length - 3);
        }
        if (num) {
            result = num + result;
        }
        return `${prefix}${result}${list[1] ? `.${list[1]}` : ''}`;
    }
    handleSuffix = () => {
        if (this.props.suffixIfEmpty) {
            let node: any = ReactDOM.findDOMNode(this);
            let input = node.querySelectorAll("input");
            if (input[0].value !== "" && !this.state.suffixIfEmpty) {
                this.setState({ suffixIfEmpty: true });
            }
        }
    }
    handleOnSearchMethod = (value: string, e?: React.KeyboardEvent<HTMLInputElement>) => {
        if (e) {
            alert("On Search Clicked!" + e.charCode);
        }
        else {
            alert("parameter is undefined");
        }
    }
    handleSearch = (value: string, event?: any) => {
        if (this.props.options && this.props.componentType === TextBoxComponentType.Search && (this.props.options as SearchOptions).onSearch) {
            (this.props.options as SearchOptions).onSearch!({ id: this.getId(), value: value, externalParamater: {} } as TextBoxSearchHandlerView);
        }
    }
    handleOnFocusOut = (event: any): void => {
        if (this.props.onFocusOut) {
            this.props.onFocusOut(event);
        }
    }
    handleAutoCompleteFilter = (inputValue: any, option: any) => {
        return option.props.children.toUpperCase().indexOf(inputValue.toUpperCase()) !== -1;
    }
    handleInputNumberKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
        const { onKeyDown } = this.props;

        // allow only [0-9] number, numpad number, arrow,  BackSpace, Tab
        const arrowsKeyCodes = [37, 38, 39, 40];
        const numPadNumberKeyCodes = [96, 97, 98, 99, 100, 101, 102, 103, 104, 105, 109, 110, 190];
        if ((e.keyCode < 48 && !arrowsKeyCodes.includes(e.keyCode) || e.keyCode > 57 && !numPadNumberKeyCodes.includes(e.keyCode)) && !(e.keyCode === 8 || e.keyCode === 9)) {
            if (!e.ctrlKey) {
                e.preventDefault();
            }
        } else {
            if (onKeyDown) {
                let controlOnKeyDownHandlerView: ControlOnKeyDownHandlerView = this.getCommonDataEntryEventArguments("handleCommonKeyDown") as ControlOnKeyDownHandlerView;
                controlOnKeyDownHandlerView.keyCode = e.keyCode;
                controlOnKeyDownHandlerView.key = e.key;
                onKeyDown(controlOnKeyDownHandlerView);
            }
        }
    }
    handleCommonKeyDown = (e: React.KeyboardEvent<any>) => {
        const { onKeyDown } = this.props;
        if (onKeyDown) {
            let controlOnKeyDownHandlerView: ControlOnKeyDownHandlerView = this.getCommonDataEntryEventArguments("handleCommonKeyDown") as ControlOnKeyDownHandlerView;
            controlOnKeyDownHandlerView.keyCode = e.keyCode;
            controlOnKeyDownHandlerView.key = e.key;
            onKeyDown(controlOnKeyDownHandlerView);
        }
    }
    handleTextAreaKeyDown = (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
        const { onKeyDown } = this.props;
        if (onKeyDown) {
            let controlOnKeyDownHandlerView: ControlOnKeyDownHandlerView = this.getCommonDataEntryEventArguments("handleTextAreaKeyDown") as ControlOnKeyDownHandlerView;
            controlOnKeyDownHandlerView.keyCode = e.keyCode;
            controlOnKeyDownHandlerView.key = e.key;
            onKeyDown(controlOnKeyDownHandlerView);
        }
    }
    handleCommonKeyUp = (e: React.KeyboardEvent<any>) => {
        const { onKeyUp } = this.props;
        if (onKeyUp) {
            let controlOnKeyUpHandlerView: ControlOnKeyUpHandlerView = this.getCommonProps() as ControlOnKeyUpHandlerView;
            controlOnKeyUpHandlerView.keyCode = e.keyCode;
            controlOnKeyUpHandlerView.key = e.key;
            onKeyUp(controlOnKeyUpHandlerView);
        }
    }
    handleTextAreaKeyUp = (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
        const { onKeyUp } = this.props;
        if (onKeyUp) {
            let controlOnKeyUpHandlerView: ControlOnKeyUpHandlerView = this.getCommonDataEntryEventArguments("handleTextAreaKeyUp") as ControlOnKeyUpHandlerView;
            controlOnKeyUpHandlerView.keyCode = e.keyCode;
            controlOnKeyUpHandlerView.key = e.key;
            onKeyUp(controlOnKeyUpHandlerView);
        }
    }
    handleCommonPressEnter = () => {
        const { onPressEnter } = this.props;
        if (onPressEnter) {
            let controlOnPressEnterHandlerView: ControlOnPressEnterHandlerView = this.getCommonProps() as ControlOnPressEnterHandlerView;
            onPressEnter(controlOnPressEnterHandlerView);
        }
    }
    handleInputPressEnter = (e: React.KeyboardEvent<HTMLInputElement>) => {
        this.handleCommonPressEnter();
    }
    handleTextAreaPressEnter = (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
        this.handleCommonPressEnter();
    }
    handleCommonKeyPress = (e: React.KeyboardEvent<HTMLInputElement>) => {
        const { onKeyPress, disabledChars } = this.props;
        if (this.props.onlyNumber && !isNumeric(e.key)) {
            e.preventDefault();
        } else if (this.props.onlyString && !isString(e.key)) {
            e.preventDefault();
        }
        if (disabledChars) {
            disabledChars.forEach(element => {
                if (element === e.key) {
                    e.preventDefault();
                    return;
                }
            });
        }
        if (onKeyPress) {
            let controlOnKeyPressHandlerView: ControlOnKeyPressHandlerView = this.getCommonProps() as ControlOnKeyPressHandlerView;
            controlOnKeyPressHandlerView.keyCode = e.keyCode;
            controlOnKeyPressHandlerView.key = e.key;
            controlOnKeyPressHandlerView.id = this.getId();
            controlOnKeyPressHandlerView.value = e.key;
            controlOnKeyPressHandlerView.bindingPath = this.getBindingPath();
            if (onKeyPress(controlOnKeyPressHandlerView) === false) {
                e.preventDefault();
            }
        }
    }
    // #endregion
    // #region (LIFE CYCLE METHODS)
    componentDidMount() {
        this.handleSuffix();
        this.handleSeleniumUnique();
    }
    componentDidUpdate() {
        this.handleSuffix();
    }
    //#endregion
    // #region (BASE RENDER (ACCORDING TO COMPONENT TYPE) METHODS)
    dataEntryControlRender(): ReactNode {
        return this.getRenderMetod();
    }

    ParseInput = (props: any) => {
        const { onChange, ...restprops } = props;
        let value = props.value ? props.value : (this.getValueProp() ? this.getValueProp()!.value : undefined);
        const [text, setText] = React.useState<any>(value);
        React.useEffect(() => {
            if (this.props.formatter) {
                value = this.props.formatter(value);
            }
            if (this.props.onlyMoney) {
                if (!text) {
                    value = manageZeroDecimalValues(value, this.props.moneyDecimal);
                }
                if (Number.isInteger(value)) {
                    value = manageZeroDecimalValues(value, this.props.moneyDecimal);
                }
                value = formatMoneyOnChange(value, this.props.moneyDecimal);
            }
            setText(value);
        }, [value]);
        // this is the place wehere we can make formatting for internal select
        const handleChange = (e: any) => {
            let value = this.props.parser ? this.props.parser(e.target.value) : e.target.value;
            if (this.props.onlyMoney) {
                value = parseMoneyOnChange(value, this.props.moneyDecimal);
            }
            this.onChange(value);
            // sets hook form values, onchange function comes from controller
            e.target.value = value;
            if (onChange) onChange(e);
            // format value before setting input value
            if (this.props.formatter) {
                value = this.props.formatter!(value);
            }
            if (this.props.onlyMoney) {
                value = formatMoneyOnChange(value, this.props.moneyDecimal);
            }
            setText(value);
        };

        const inputOptions = this.getOptions() as InputOptions;
        let defaultValueProp = this.getDefaultValueProp();

        let valueProp = (this.isComponentHasBindingPathProp() || this.props.formatter || this.props.onlyMoney) ? { value: text } : this.getValueProp();
        let suffixButton = <button className="textbox-custom-suffix-icon" onClick={this.props.onSuffixClick}>{this.props.suffixText}</button>;
        let suffix = this.state.suffixIfEmpty ? (this.props.suffixText ? suffixButton : (this.props.suffix ? this.props.suffix : "")) : "";

        return <Input
            {...restprops}
            {...defaultValueProp}
            {...valueProp}
            maxLength={inputOptions.maxLength}
            prefix={inputOptions.prefix}
            suffix={suffix}
            allowClear={this.props.disabled ? false : inputOptions.allowClear}
            disabled={this.props.disabled}
            onChange={handleChange}
            onPressEnter={this.handleInputPressEnter}
            onBlur={this.handleOnBlur}
            onKeyDown={this.handleCommonKeyDown}
            onKeyUp={this.handleCommonKeyUp}
            onKeyPress={this.handleCommonKeyPress}
            tabIndex={this.props.tabIndex}
        />;
    }
    ParseInputNumber = (props: any) => {
        const { onChange, ...restprops } = props;
        let value = props.value ? props.value : (this.getValueProp() ? this.getValueProp()!.value : undefined);
        const [text, setText] = React.useState<any>(value);
        React.useEffect(() => {
            setText(value);
        }, [value]);
        // this is the place wehere we can make formatting for internal select
        const handleChange = (e: any) => {
            let value = e;
            this.onChange(value);
            // sets hook form values, onchange function comes from controller
            e = value;
            if (onChange) onChange(e);
            // format value before setting input value
            setText(value);
        };

        const inputNumberOptions = this.getOptions() as InputNumberOptions;
        let defaultValueProp = this.getDefaultValueProp();

        let valueProp = this.isComponentHasBindingPathProp() ? { value: text } : this.getValueProp();

        return <InputNumber
            {...restprops}
            {...valueProp}
            {...defaultValueProp}
            maxLength={inputNumberOptions.maxLength}
            decimalSeparator={inputNumberOptions.decimalSeparator}
            disabled={this.props.disabled}
            step={inputNumberOptions.step}
            min={inputNumberOptions.minValue}
            max={inputNumberOptions.maxValue}
            onChange={handleChange}
            onBlur={this.handleOnBlur}
            onKeyDown={this.handleInputNumberKeyDown}
            onKeyUp={this.handleCommonKeyUp}
            onKeyPress={this.handleCommonKeyPress}
            tabIndex={this.props.tabIndex}
        />;
    }
    renderInput(commonProps: any): ReactNode {
        return (
            <>
                {this.renderElement(
                    <this.ParseInput {...commonProps} />,
                    this.getBindingPath(),
                    this.onChangeInput,
                    this.props.rules
                )}
                {this.props.placeHolderMessage && !this.props.groupElement && <p style={{ marginTop: "5px", fontSize: "12px", marginLeft: "2px" }}>{`"${this.props.placeHolderMessage}"`}</p>}
            </>
        );
    }
    renderInputNumber(commonProps: any): ReactNode {
        return (
            <>
                {this.renderElement(
                    <this.ParseInputNumber {...commonProps} />,
                    this.getBindingPath(),
                    this.onChangeInputNumber,
                    this.props.rules
                )}
                {this.props.placeHolderMessage && !this.props.groupElement && <p style={{ marginTop: "5px", fontSize: "12px", marginLeft: "2px" }}>{`"${this.props.placeHolderMessage}"`}</p>}
            </>
        );
    }

    renderSearch(commonProps: any): ReactNode {
        const searchOptions = this.getOptions() as SearchOptions;
        let valueProp = this.getValueProp();
        let defaultValueProp = this.getDefaultValueProp();
        let suffixButton = <button className="textbox-custom-suffix-icon" onClick={this.props.onSuffixClick}>{this.props.suffixText}</button>;
        let suffix = this.state.suffixIfEmpty ? this.props.suffixText ? suffixButton : this.props.suffix ? this.props.suffix : undefined : undefined;
        return (
            <>
                {this.renderElement(
                    <Search
                        {...commonProps}
                        {...valueProp}
                        {...defaultValueProp}
                        disabled={this.props.disabled}
                        suffix={suffix}
                        prefix={searchOptions.prefix}
                        maxLength={searchOptions.maxLength}
                        enterButton={searchOptions.enterButton}
                        onChange={this.onChange}
                        onPressEnter={this.handleInputPressEnter}
                        onBlur={this.handleOnBlur}
                        onSearch={this.handleSearch}
                        tabIndex={this.props.tabIndex}
                    />,
                    this.getBindingPath(),
                    this.onChangeInput,
                    this.props.rules,
                    undefined,
                    undefined,
                    "onBlur"
                )}
                {this.props.placeHolderMessage && !this.props.groupElement && <p style={{ marginTop: "5px", fontSize: "12px", marginLeft: "2px" }}>{`"${this.props.placeHolderMessage}"`}</p>}
            </>
        );
    }



    renderDetailSearch(commonProps: any): ReactNode {
        const detailSearchOptions = this.getOptions() as DetailSearchOptions;

        let valueProp = this.getValuePropForDetailSearch();
        let value = (valueProp !== undefined && valueProp.value) ? valueProp.value : "";
        let defaultValueProp = this.getDefaultValueProp();
        let suffixButton = <Button className="textbox-custom-suffix-icon" style={{ maxHeight: '40px', marginRight: '-6px' }} icon={this.props.suffixIcon} onClick={this.handleSuffixButtonClick}>{this.props.suffixText}</Button>;
        let classIdentifier = "textbox-custom-suffix-icon-detail-search";
        let suffix = this.state.suffixIfEmpty ? this.props.suffixText ? suffixButton : this.props.suffix ? this.props.suffix : undefined : undefined;
        this.isDetailSearchActive = false;
        return (
            <>
                {this.renderElement(
                    <Input
                        {...commonProps}
                        {...valueProp}
                        {...defaultValueProp}
                        allowClear={detailSearchOptions.allowClear}
                        disabled={this.props.disabled}
                        suffix={suffix}
                        prefix={detailSearchOptions.prefix}
                        maxLength={detailSearchOptions.maxLength}
                        enterButton={detailSearchOptions.enterButton}
                        onChange={this.onChange}
                        onPressEnter={this.handleInputPressEnter}
                        onBlur={this.handleOnBlur}
                        onSearch={this.handleSearch}
                        tabIndex={this.props.tabIndex}
                        className={classIdentifier}
                    />,
                    this.getBindingPath(),
                    this.onChangeInput,
                    this.props.rules,
                    undefined,
                    undefined,
                    "onBlur"
                )}
                {this.props.placeHolderMessage && !this.props.groupElement && <p style={{ marginTop: "5px", fontSize: "12px", marginLeft: "2px" }}>{`"${this.props.placeHolderMessage}"`}</p>}
                <TextboxDetailSearch id="" searchValue={value} visible={this.state.isSearchPopupVisible} onClosePopup={this.searchPopupOnClose} detailSearchFound={this.detailSearchFound} popupSearchTitle={detailSearchOptions.popupSearchTitle}
                    popupInputLabel={detailSearchOptions.popupInputLabel} searchButtonText={detailSearchOptions.searchButtonText} dataSource={detailSearchOptions.dataSource} dataGridColumns={detailSearchOptions.dataGridColumns} />
            </>

        );
    }
    detailSearchFound = (item: any) => {
        let detailSearchOptions = this.getOptions() as DetailSearchOptions;
        let value = "";
        if (!isUndefined(item) && item.Value) {
            if (detailSearchOptions.showValueWithCode) {
                value = item.Code + "-" + item.Value;
            } else {
                value = item.Value!;
            }
        }
        if (!this.isComponentHasBindingPathProp()) {
            this.isDetailSearchActive = true;
            this.setState({ detailSearchSelectedValue: value });
            if (this.props.onChangeValue) {
                this.props.onChangeValue({ id: this.getId(), value: value, externalParameter: this.props.externalParameter } as CommonDataEntryEventArguments);
            }
        } else {
            this.setFormDataField(value, this.getBindingPath());
        }
    }


    searchPopupOnClose = (selectedValue: any) => {
        this.setState({ isSearchPopupVisible: false });
    }
    renderTextArea(commonProps: any): ReactNode {
        const textAreaOptions = this.getOptions() as TextAreaOptions;
        let valueProp = this.getValueProp();
        let defaultValueProp = this.getDefaultValueProp();
        return (
            <>
                {this.renderElement(
                    <TextArea
                        {...commonProps}
                        {...valueProp}
                        {...defaultValueProp}
                        maxLength={textAreaOptions.maxLength}
                        disabled={this.props.disabled}
                        autosize={textAreaOptions.autosize}
                        onChange={this.onChange}
                        onBlur={this.handleOnBlur}
                        onKeyDown={this.handleTextAreaKeyDown}
                        onKeyUp={this.handleTextAreaKeyUp}
                        tabIndex={this.props.tabIndex}
                    />,
                    this.getBindingPath(),
                    this.onChangeInput,
                    this.props.rules,
                    undefined,
                    undefined,
                    "onBlur"
                )}
                {this.props.placeHolderMessage && !this.props.groupElement && <p style={{ marginTop: "5px", fontSize: "12px", marginLeft: "2px" }}>{`"${this.props.placeHolderMessage}"`}</p>}
            </>
        );
    }
    renderAutoComplete(commonProps: any): ReactNode {
        let autoCompleteOptions = this.getOptions() as AutoCompleteOptions;
        let valueProp = this.getValueProp();
        let defaultValueProp = this.getDefaultValueProp();
        return (
            <>
                {this.renderElement(
                    <AutoComplete
                        {...commonProps}
                        {...valueProp}
                        {...defaultValueProp}
                        dataSource={autoCompleteOptions.dataSource}
                        autoFocus={autoCompleteOptions.autoFocus}
                        backfill={autoCompleteOptions.backfill}
                        onSelect={autoCompleteOptions.onSelect}
                        children={autoCompleteOptions.children}
                        onSearch={autoCompleteOptions.onSearch}
                        optionLabelProp={autoCompleteOptions.optionLabelProp}
                        disabled={this.props.disabled}
                        onChange={this.onChange}
                        onBlur={this.handleOnBlur}
                        filterOption={this.handleAutoCompleteFilter}
                        tabIndex={this.props.tabIndex}
                    />,
                    this.getBindingPath(),
                    this.onChangeAutoComplete,
                    this.props.rules
                )}
                {this.props.placeHolderMessage && !this.props.groupElement && <p style={{ marginTop: "5px", fontSize: "12px", marginLeft: "2px" }}>{`"${this.props.placeHolderMessage}"`}</p>}
            </>
        );
    }
    renderPassword(commonProps: any): ReactNode {
        let inputOptions = this.getOptions() as InputOptions;
        let suffixButton = <button className="textbox-custom-suffix-icon" onClick={this.props.onSuffixClick}>{this.props.suffixText}</button>;
        let suffix: any = this.state.suffixIfEmpty ? this.props.suffixText ? suffixButton : this.props.suffix ? this.props.suffix : inputOptions.suffix : "";
        let valueProp = this.getValueProp();
        let defaultValueProp = this.getDefaultValueProp();
        return (
            this.renderElement(
                <Input.Password
                    {...commonProps}
                    {...valueProp}
                    {...defaultValueProp}
                    suffix={suffix}
                    maxLength={inputOptions.maxLength}
                    onChange={this.onChange}
                    onBlur={this.handleOnBlur}
                    tabIndex={this.props.tabIndex}
                />,
                this.getBindingPath(),
                this.onChangePassword
            )
        );
    }
    //#endregion
}

const textbox = surfaceContext<TextBoxProps>(surfaceForm<TextBoxProps>(TextBox));
export { textbox as TextBox };