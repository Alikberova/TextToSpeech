import { CommonModule } from '@angular/common';
import { Component, Input, Output, EventEmitter } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';

@Component({
  selector: 'app-dropdown',
  standalone: true,
  imports: [FormsModule, MatIconModule, MatSelectModule, CommonModule],
  templateUrl: './dropdown.component.html',
  styleUrls: ['./dropdown.component.scss']
})

export class DropdownComponent {
  @Input() id!: string;
  @Input() label: string | undefined;
  @Input() options!: string[];
  @Input() selectedOption: string | undefined;
  @Input() matIcon: string | undefined;
  @Input() clickedMatIcon: string | undefined;
  @Output() iconClick: EventEmitter<{ event: MouseEvent, index: number }> = new EventEmitter<{ event: MouseEvent, index: number }>();
  clickedOptionIndex: any;

  onIconClick(event: MouseEvent): void {
    const index = (event.target as HTMLElement).getAttribute('data-index');
    if (index !== null) {
        this.clickedOptionIndex = parseInt(index, 10);
        this.iconClick.emit({ event, index: this.clickedOptionIndex });
    }
  }

  getMatIconName(option: string) {
    const isIconClickedOnThisOption = option === this.options[this.clickedOptionIndex];
    
    return isIconClickedOnThisOption && this.clickedMatIcon ? this.clickedMatIcon : this.matIcon
  }
}
