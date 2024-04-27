import { Component, Output, EventEmitter, Input, SimpleChanges, ViewChild, ElementRef} from '@angular/core';

@Component({
  selector: 'toggle-button',
  standalone: true,
  templateUrl: './toggle-button.component.html',
  styleUrl: './toggle-button.component.scss'
})
export class ToggleButtonComponent  {
  @Output() changed = new EventEmitter<boolean>();
  @Input() isRegister: boolean = false;
  
  label: string = '';

  @ViewChild('checkbox') checkbox!: ElementRef;

  ngOnChanges(changes: SimpleChanges) {
    if (changes['isRegister']) {
      this.updateToggleState();
      this.label = this.isRegister ? 'Register' : 'Login';
    }
  }

  ngAfterViewInit() {
    this.updateToggleState();
  }

  onCheckboxChange(event: Event) {
    const target = event.target as HTMLInputElement;
    this.isRegister = target.checked;
    this.changed.emit(this.isRegister);
  }

  updateToggleState() {
    // Update the checkbox's checked property to match isRegister
    if (this.checkbox) {
      this.checkbox.nativeElement.checked = this.isRegister;
    }
  }
}