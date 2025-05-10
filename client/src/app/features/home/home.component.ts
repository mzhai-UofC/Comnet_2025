import { Component } from '@angular/core';
import { CommonModule } from '@angular/common'; // 导入 CommonModule
import { MatButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';

@Component({
  selector: 'app-home',
  standalone: true, // 声明为 standalone 组件
  imports: [CommonModule,
    MatButton,
    MatIcon
  ], // 添加 CommonModule
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent {
  isModalOpen = false;
  isChapter = false; // 用于区分章节和最近更新
  currentChapterPages: string[] = [];
  currentChapterIndex = 0;

  chapter1 = {
    name: 'Chapter 1: Wheel of Fate',
    thumbnail: '/assets/images/comic-thumbnail-1.jpg',
    pages: [
      '/images/comics/comic1.jpg',
      '/images/comics/comic2.jpg',
      '/images/comics/comic3.jpg'
    ]
  };

  chapter2 = {
    name: 'Chapter 2: Unusual Friends',
    thumbnail: '/assets/images/comic-thumbnail-2.jpg',
    pages: [
      '/images/comics/comic4.jpg',
      '/images/comics/comic5.jpg',
      '/images/comics/comic6.jpg'
    ]
  };

  chapter3 = {
    name: 'Chapter 3: Cats Hold Grudges',
    thumbnail: '/assets/images/comic-thumbnail-3.jpg',
    pages: [
      '/images/comics/comic7.jpg',
      '/images/comics/comic8.jpg',
      '/images/comics/comic9.jpg'
    ]
  };

  chapter4 = {
    name: "Chapter 4: Panpan's Wonderland",
    thumbnail: '/assets/images/comic-thumbnail-4.jpg',
    pages: [
      '/images/comics/comic14.jpg',
      '/images/comics/comic15.jpg',
      '/images/comics/comic17.jpg'
    ]
  };

  // 定义最近更新数据
  recentUpdate = {
    name: 'Happy Easter 2025',
    pages: [
      '/images/comics/easter2025.jpg' // 最近更新只有一张图片
    ]
  };

  // 打开模态窗口
openModal(content: any): void {
  this.currentChapterPages = content.pages;
  this.isChapter = [this.chapter1, this.chapter2, this.chapter3, this.chapter4].includes(content);
  this.currentChapterIndex = this.isChapter
    ? [this.chapter1, this.chapter2, this.chapter3, this.chapter4].indexOf(content)
    : -1;
  this.isModalOpen = true;

  // 确保模态窗口内容加载完成后滚动到顶部
  setTimeout(() => {
    const modalElement = document.querySelector('.modal-content');
    if (modalElement) {
      modalElement.scrollTo({ top: 0, behavior: 'smooth' });
    } else {
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  }, 100); // 延迟 100ms 确保内容加载完成
}


  // 关闭模态窗口
  closeModal(): void {
    this.isModalOpen = false;
    this.currentChapterPages = [];
  }

  // 跳转到下一个章节
goToNextStory(): void {
  const chapters = [this.chapter1, this.chapter2, this.chapter3, this.chapter4];
  const nextIndex = this.currentChapterIndex + 1;
  if (nextIndex < chapters.length) {
    const nextChapter = chapters[nextIndex];
    this.openModal(nextChapter);
  } else {
    alert('This is the last chapter!');
  }
}
}