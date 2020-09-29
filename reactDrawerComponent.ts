
import * as React from 'react';
import ReactDOM from 'react-dom';
import { DataDisplayControlProps, DataDisplayControlState, DataDisplayControl } from '../../../base/data-display-control';
import { surfaceContext } from "../../../base/hoc/hoc";
import { DrawerConstants } from './drawer-constants';
import { Drawer } from 'antd';
import './drawer-css.css';

export interface DrawerProps extends DataDisplayControlProps {
    showCloseButton?: boolean;
    placement?: 'left' | 'right' | 'bottom' | 'top';
    maskClosable?: boolean;
    mask?: boolean;
    maskStyle?: React.CSSProperties;
    style?: React.CSSProperties;
    bodyStyle?: object;
    title?: string | React.ReactNode;
    width?: string | number;
    height?: string | number;
    zIndex?: number;
    destroyOnClose?: boolean;
    onClose: () => void | undefined;
    children: any;

}

export interface DrawerState extends DataDisplayControlState {
}

class SurfaceDrawer extends DataDisplayControl<DrawerProps, DrawerState> {
    static defaultProps = {

        showCloseButton: DrawerConstants.DefaultDrawerCloseButtonVisibilty,
        maskClosable: DrawerConstants.DefaultDrawerMaskClosable,
        mask: DrawerConstants.DefaultDrawerIsHasMask,
        title: DrawerConstants.DefaultDrawerTitle,
        placement: DrawerConstants.DefaultDrawerPlacement,
        width: DrawerConstants.DefaultDrawerWidth,
        height: DrawerConstants.DefaultDrawerHeight,
        zIndex: DrawerConstants.DefaultDrawerZindex,
        useEscToClose: DrawerConstants.DefaultDrawerUsingEscToClose,
        destroyOnClose: DrawerConstants.DefaultDrawerDestroyOnClose,
        forceRender: DrawerConstants.DefaultDrawerForceRender
    };

    constructor(props: any) {
        super(props);
        this.state = {
        };
    }

    getComponentType() {
        return "drawer";
    }


    dataDisplayControlRender(): React.ReactNode {
        let className = "surface-drawer";
        return (
            <>
                <Drawer
                    closable={this.props.showCloseButton}
                    title={this.props.title}
                    placement={this.props.placement}
                    onClose={this.props.onClose}
                    visible={this.props.visible}
                    style={this.props.style}
                    mask={this.props.mask}
                    maskClosable={this.props.maskClosable}
                    maskStyle={this.props.maskStyle}
                    bodyStyle={this.props.bodyStyle}
                    width={this.props.width}
                    height={this.props.height}
                    zIndex={this.props.zIndex}
                    className={className}
                >{this.props.children}</Drawer>
            </>);

    }
}

const drawer = surfaceContext<DrawerProps>(SurfaceDrawer);
export { drawer as Drawer };
