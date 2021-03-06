﻿/**
 * Guid
 */
export class Guid {
    private array: string[] = [];

    constructor(input?: string) {
        if (input && typeof (input) === 'string') {
            input = input.replace(/\{|\(|\)|\}|\-/g, '');
            input = input.toLowerCase();
            if (input.length === 32 && input.search(/[^0-9,a-f]/i) < 0) {
                for (let i = 0; i < input.length; i++) {
                    this.array.push(input[i]);
                }
            }
        }
        if (this.array.length === 0) {
            for (let i = 0; i < 32; i++) {
                this.array.push('0');
            }
        }
    }

    /**
     * Guid对象的标记
     */
    isGuid: boolean = true;

    /**
     * 返回一个值，该值指示 Guid 的两个实例是否表示同一个值
     * @param other
     * @returns
     */
    equals(other: Guid): boolean {
        if (other && other.isGuid)
            return this.toString() == other.toString();
        return false;
    };

    /**
     * 返回 Guid 类的此实例值的 String 表示形式。
     * 根据所提供的格式说明符，返回此 Guid 实例值的 String 表示形式。
     * N  32 位： xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
     * D  由连字符分隔的 32 位数字 xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
     * B  括在大括号中、由连字符分隔的 32 位数字：{xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx}
     * P  括在圆括号中、由连字符分隔的 32 位数字：(xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx)
     * @param format
     * @returns
     */
    toString(format?: string): string {
        if (format)
            switch (format) {
                case 'N':
                    return this.array.toString().replace(/,/g, '');
                case 'D':
                    return `${this.array.slice(0, 8)}-${this.array.slice(8, 12)}-${this.array.slice(12, 16)}-${this.array.slice(16, 20)}-${this.array.slice(20, 32)}`.replace(/,/g, '');
                case 'B':
                    return `{${this.toString('D')}}`;
                case 'P':
                    return `(${this.toString('D')})`;
                default:
                    throw new Error("Parameter “format” must be one of N|D|B|P]");
            }
        else
            return this.toString('D');
    }

    /**
     * Guid 类的默认实例，其值保证均为零
     */
    static empty: Guid = new Guid();

    /**
     * 初始化 Guid 类的一个新实例
     */
    static newGuid(): Guid {
        let string = '';
        for (let i = 0; i < 32; i++) {
            string += Math.floor(Math.random() * 16.0).toString(16);
        }
        return new Guid(string);
    }
}