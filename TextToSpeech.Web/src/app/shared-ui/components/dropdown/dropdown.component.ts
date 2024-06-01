import { CommonModule } from '@angular/common';
import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { DropdownConfig, DropdownItem } from '../../../models/dropdown-config';

@Component({
  selector: 'app-dropdown',
  standalone: true,
  imports: [FormsModule, MatIconModule, MatSelectModule, CommonModule],
  templateUrl: './dropdown.component.html',
  styleUrls: ['./dropdown.component.scss']
})

export class DropdownComponent implements OnChanges {
  static readonly activeMatIconClass: string = 'active-mat-icon';
  @Input() config!: DropdownConfig;
  @Input() matIcon: string | undefined;
  @Input() clickedMatIcon: string | undefined;
  @Input() clickedMatIconClass: string | undefined;
  @Output() selectionChanged = new EventEmitter<any>();
  @Output() iconClick: EventEmitter<{ event: MouseEvent, index: number }> = new EventEmitter<{ event: MouseEvent, index: number }>();
  clickedOptionIndex: number = undefined!;
  dropDownList: DropdownItem[] = [];
  selectedIndex: number | undefined;
  valueField: string | undefined;
  labelField: string | undefined;
  heading!: string;
  id!: string;

  ngOnChanges(changes: SimpleChanges) {
    if (changes['config']) {
      this.dropDownList = this.config.dropDownList || [];
      this.selectedIndex = this.config.selectedIndex ?? -1;
      this.valueField = this.config.valueField || 'value';
      this.labelField = this.config.labelField || 'label';
      this.heading = this.config.heading || '';
      this.id = this.heading.replace(' ', '-').toLowerCase() + '-dropdown';
    }
  }

  onSelectionChange() {
    this.selectionChanged.emit(this.selectedIndex);
  }

  onIconClick(event: MouseEvent): void {
    const index = (event.target as HTMLElement).getAttribute('data-index');
    if (index !== null) {
        this.clickedOptionIndex = parseInt(index, 10);
        this.iconClick.emit({ event, index: this.clickedOptionIndex });
    }
  }

  getMatIconName(option: DropdownItem) {
    return this.isIconClickedOnThisOption(option) && this.clickedMatIcon ? this.clickedMatIcon : this.matIcon
  }

  getMatIconClass(option: DropdownItem ) {
    if (this.isIconClickedOnThisOption(option)) {
      return this.clickedMatIconClass ? this.clickedMatIconClass : DropdownComponent.activeMatIconClass;
    }
    return '';
  }

  isIconClickedOnThisOption(option: DropdownItem ): boolean {
    if (this.clickedOptionIndex === undefined) {
      return false;
    }
    const clickedOption = this.dropDownList[this.clickedOptionIndex];
    if (!clickedOption) {
      return false;
    }
    return option.id === clickedOption.id;
  }
}
