import { Pipe, PipeTransform } from "@angular/core";

@Pipe({
    name: "FormatMaxInputLengthPipe",
    standalone: true,
})

export class FormatMaxInputLengthPipe implements PipeTransform{

  transform(maxInputLength: number): string {
    if (maxInputLength > 9999) {
        const formattedLength = maxInputLength.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ' ');
        return formattedLength;
    } else {
        return maxInputLength.toString();
    }
  }
}
