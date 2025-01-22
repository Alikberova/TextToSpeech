export interface DropdownConfig {
    selectedIndex: number;
    valueField: string;
    labelField: string;
    dropDownList: DropdownItem[];
    heading: string;
}

export interface DropdownItem {
  id: number;
  optionDescription: string;
}